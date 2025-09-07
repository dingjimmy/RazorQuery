using Microsoft.Extensions.Caching.Memory;

namespace RazorQuery;

/// <summary>
/// Represents an asynchronous operation that can retrieve data that meets a specific criteria.
/// </summary>
/// <remarks>This class encapsulates a data retrieval operation, allowing the caller to execute the operation
/// asynchronously and track its status.
/// </remarks>
/// <typeparam name="T">The type of data to be retrieved.</typeparam>
/// <typeparam name="TFilter">The type representing the criteria used to filter the data being retrieved.</typeparam>
public class Query<T, TFilter> : Query<T, TFilter, DefaultQueryFunctionContext>
{
    public Query(Func<TFilter, DefaultQueryFunctionContext, Task<T>> queryFunction, DefaultQueryFunctionContext queryFunctionContext)
        : base(queryFunction, queryFunctionContext)
    {
    }
}


/// <summary>
/// Represents an asynchronous operation that can retrieve data that meets a specific criteria.
/// </summary>
/// <remarks>This class encapsulates a data retrieval operation, allowing the caller to execute the operation
/// asynchronously and track its status.
/// </remarks>
/// <typeparam name="T">The type of data to be retrieved.</typeparam>
/// <typeparam name="TFilter">The type representing the criteria used to filter the data being retrieved.</typeparam>
/// <typeparam name="TQueryFunctionContext">The type of query context to pass to the query function, providing access to necessary resources.</typeparam>
public class Query<T, TFilter, TQueryFunctionContext> : QueryBase 
    where TQueryFunctionContext : notnull, IQueryFunctionContext
    {
        private readonly Func<TFilter, TQueryFunctionContext, Task<T>> _queryFunc;
        private readonly TQueryFunctionContext _queryFuncContext;

        public T? Data { get; protected set; }


        public Query(Func<TFilter, TQueryFunctionContext, Task<T>> queryFunction, TQueryFunctionContext queryFunctionContext)
        {
            _queryFunc = queryFunction ?? throw new ArgumentNullException(nameof(queryFunction));
            _queryFuncContext = queryFunctionContext ?? throw new ArgumentNullException(nameof(queryFunctionContext));
        }

        public async Task<T> Execute(TFilter filter)
        {
            Error = null;
            Status = QueryStatus.Pending;

            try
            {
                Data = await _queryFunc(filter, _queryFuncContext);

                // check for errors raised by the function itself.
                // TODO: currently using exceptions for flow logic, but should be 
                //       replaced with a more structured error handling approach
                if (_queryFuncContext.ErrorMessage.Length > 0)
                {
                    throw new Exception(_queryFuncContext.ErrorMessage);
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