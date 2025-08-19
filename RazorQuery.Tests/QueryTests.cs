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
    }

    [Fact]
    public void Query_is_in_an_idle_state_before_query_function_is_executed()
    {
        // Arrange
        var queryFactory = _serviceCollection
            .AddRazorQuery()
            .BuildServiceProvider()
            .GetRequiredService<QueryFactory>();

        var query = queryFactory.Create<TestData, string>(
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
        var queryFactory = _serviceCollection
            .AddRazorQuery()
            .BuildServiceProvider()
            .GetRequiredService<QueryFactory>();

        var query = queryFactory.Create<TestData, string>(
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
    public async Task QueryFunction_can_make_http_requests()
    {
        // Arrange
        var queryFactory = _serviceCollection
            .AddRazorQuery()
            .BuildServiceProvider()
            .GetRequiredService<QueryFactory>();

        var query = queryFactory.Create<TestData, string>(
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
        Assert.Equal("Testing is cool!", query.Data.Result);

    }
}

public class TestData
{
    public string? Result { get; set; }
}