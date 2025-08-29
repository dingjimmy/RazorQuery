using Microsoft.Extensions.DependencyInjection;
using Moq;
using RichardSzalay.MockHttp;
using System.Net;
using Xunit.Sdk;

namespace RazorQuery.Tests;

public class QueryTests
{
    private readonly MockHttpMessageHandler _httpMessageHandler = new();
    private readonly IHttpClientFactory _httpClientFactory = Mock.Of<IHttpClientFactory>();
    private readonly IServiceCollection _serviceCollection;

    public QueryTests()
    {
        _httpMessageHandler.When("http://razor-query-tests.com")
            .Respond(HttpStatusCode.OK, "application/text", "Testing is cool!");

        Mock.Get(_httpClientFactory).Setup(x => x.CreateClient(string.Empty))
            .Returns(_httpMessageHandler.ToHttpClient());

        _serviceCollection = new ServiceCollection()
           .AddSingleton(_httpClientFactory);
        
        var serviceProvider = _serviceCollection
            .AddRazorQuery()
            .BuildServiceProvider();

        QueryFactory.SetServiceProvider(serviceProvider);
    }

    [Fact]
    public void Query_is_in_an_idle_state_before_query_function_is_executed()
    {
        // Arrange
        var query = QueryFactory.Create<TestData, string>(
            async (filter, context) =>
            {
                await Task.CompletedTask; // Simulate some processing
                throw new InvalidOperationException("This query function should not have been executed!");
            });

        // Act - do not execute the query!

        // Assert
        Assert.True(query.IsIdle);
        Assert.Equal(QueryStatus.Idle, query.Status);
        Assert.Null(query.Error);
    }

    [Fact]
    public async Task Query_goes_into_success_state_if_query_function_completes_with_no_errors()
    {
        // Arrange
        var query = QueryFactory.Create<TestData, string>(
            async (filter, context) =>
            {
                await Task.CompletedTask; // Simulate some processing
                return new TestData();
            });

        // Act
        await query.Execute("test filter");

        // Assert
        Assert.True(query.IsSuccess);
        Assert.Equal(QueryStatus.Success, query.Status);
        Assert.Null(query.Error);
    }

    [Fact]
    public async Task Query_goes_into_error_state_if_ErrorMessage_is_set_by_query_function()
    {
        // Arrange
        var query = QueryFactory.Create<TestData, string>(
            async (filter, context) =>
            {
                await Task.CompletedTask; // Simulate some processing
                context.ErrorMessage = "An error occurred during execution.";
                return new TestData();
            });

        // Act
        await query.Execute("test filter");

        // Assert
        Assert.True(query.IsError);
        Assert.Equal(QueryStatus.Error, query.Status);
        Assert.Equal("An error occurred during execution.", query.Error!.Message);
    }

    [Fact]
    public async Task Query_goes_into_error_state_if_Exception_is_thrown_by_query_function()
    {
        // Arrange
        var query = QueryFactory.Create<TestData, string>(
            async (filter, context) =>
            {
                await Task.CompletedTask; // Simulate some processing
                throw new Exception("An error occurred during execution.");
            });

        // Act
        await query.Execute("test filter");

        // Assert
        Assert.True(query.IsError);
        Assert.Equal(QueryStatus.Error, query.Status);
        Assert.Equal("An error occurred during execution.", query.Error!.Message);
    }

    [Fact]
    public async Task Query_is_in_pending_state_while_the_query_function_is_executing()
    {
        // PLEASE READ ME:
        // This test is verifying the state of the query while the query
        // function is being executed, which isn't easy to do. Currently using
        // ManualResetEvents to synchronise the test execution, but there may
        // be other more succinct approaches to achieve this. Open to suggestions.


        // Arrange
        var pauseAct = new ManualResetEvent(false);
        var pauseAssert = new ManualResetEvent(false);

        var query = QueryFactory.Create<TestData, string>(
            async (filter, context) =>
            {
                await Task.CompletedTask; // Simulate some processing
                pauseAssert.Set();        // Allow the assert to proceed
                pauseAct.WaitOne();       // Wait until the assert completes
                return new TestData();
            });

        // Act
        var actTask = Task.Run(async () =>
        {
            await query.Execute("test filter");
        });

        // Assert
        var assertTask = Task.Run(() =>
        {
            pauseAssert.WaitOne(); // Wait until the query function is executing

            try
            {
                Assert.True(query.IsPending);
                Assert.Equal(QueryStatus.Pending, query.Status);
                Assert.Null(query.Error);
            }
            finally
            {
                pauseAct.Set(); // Allow the query function to complete
            }
        });

        await Task.WhenAll(actTask, assertTask);
    }

    [Fact]
    public async Task QueryFunction_can_make_http_requests()
    {
        // Arrange
        var query = QueryFactory.Create<TestData, string>(
            async (filter, context) =>
            {
                var response = await context.HttpClient.GetStringAsync("http://razor-query-tests.com");

                return new TestData()
                { 
                    Result = response
                };
            });

        // Act
        await query.Execute("test filter");

        // Assert
        Assert.Equal("Testing is cool!", query.Data?.Result);

    }
}

public class TestData
{
    public string? Result { get; set; }
}