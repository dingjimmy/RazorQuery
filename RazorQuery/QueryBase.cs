using Microsoft.Extensions.DependencyInjection;

namespace RazorQuery;

/// <summary>
/// Represents the base class for query operations, providing common properties and 
/// status checks.
/// </summary>
/// <remarks>This class serves as a foundation for implementing query-related 
/// functionality. It includes properties to track the query's status, any associated 
/// error, and convenience properties to simplify Razor Component code. Derived classes 
/// should implement specific query logic while utilizing the provided status and 
/// error tracking.</remarks>
public abstract class QueryBase
{
    public QueryStatus Status { get; protected set; } = QueryStatus.Idle;

    public Exception? Error { get; protected set; } = default;


    public bool IsIdle => Status == QueryStatus.Idle;

    public bool IsPending => Status == QueryStatus.Pending;

    public bool IsSuccess => Status == QueryStatus.Success;

    public bool IsError => Status == QueryStatus.Error;
}

/// <summary>
/// Represents the context for executing a query function. Intended to be used by query 
/// functions to access necessary resources and for reporting errors.
/// </summary>
public class DefaultQueryFunctionContext : IQueryFunctionContext
{
    private readonly IServiceProvider _ServiceProvider;

    public string ErrorMessage { get; set; } = string.Empty;

    public HttpClient HttpClient { get; }

    public DefaultQueryFunctionContext(IServiceProvider serviceProvider)
    {
        _ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        HttpClient = serviceProvider.GetRequiredService<HttpClient>();
    }

    public T? GetService<T>()
    {
        return _ServiceProvider.GetService<T>();
    }

    public T GetRequiredService<T>() where T : notnull
    {
        return _ServiceProvider.GetRequiredService<T>();
    }
}

public interface IQueryFunctionContext
{
    string ErrorMessage { get; set; }
}

/// <summary>
/// Represents the current state of a query.
/// </summary>
public enum QueryStatus
{
    Idle,
    Pending,
    Success,
    Error
}
