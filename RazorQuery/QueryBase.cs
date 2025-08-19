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
public class QueryFunctionContext
{
    public QueryFunctionContext(HttpClient httpClient)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient), "HttpClient cannot be null.");
    }

    public string ErrorMessage { get; set; } = string.Empty;

    public HttpClient HttpClient { get; set; }

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
