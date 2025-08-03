namespace RazorQuery;

/// <summary>
/// Represents an asynchronous operation that can retrieve data that meets a specific criteria.
/// </summary>
/// <remarks>This class encapsulates a data retrieval operation, allowing the caller to execute the operation
/// asynchronously and track its status.
/// </remarks>
/// <typeparam name="T">The type of data to be retrieved.</typeparam>
/// <typeparam name="TFilter">The type representing the criteria used to filter the data being retrieved.</typeparam>
public class Query<T, TFilter> : QueryBase
    where T : new()
{
    private readonly Func<TFilter, QueryFunctionContext, Task<T>> _queryFunc;


    public T Data { get; protected set; }


    public Query(Func<TFilter, QueryFunctionContext, Task<T>> queryFunction)
    {
        Data = new T();
        _queryFunc = queryFunction ?? throw new ArgumentNullException(nameof(queryFunction));
    }

    public async Task<T> Execute(TFilter filter)
    {
        Error = null;
        Status = QueryStatus.Pending;


        // TODO: Link this up to DI container or HttpClient factory
        var funcContext = new QueryFunctionContext
        {
            ErrorMessage = string.Empty,
            HttpClient = new HttpClient() // this is v bad, should be injected instead. just using for prototype.
        };

        try
        {
            Data = await _queryFunc(filter, funcContext);

            // check for errors raised by the function itself.
            // TODO: currently using exceptions for flow logic, but should be 
            //       replaced with a more structured error handling approach
            if (funcContext.ErrorMessage.Length > 0)
            {
                throw new Exception(funcContext.ErrorMessage);
            }

            Status = QueryStatus.Success;
            return Data;
        }
        catch (Exception e)
        {
            Status = QueryStatus.Error;
            Error = e;
            return default!;
        }
    }
}