using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.CodeAnalysis;

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
    where T : class
    {
        public Query(DefaultQueryFunctionContext queryFunctionContext, IMemoryCache memoryCache)
            : base(queryFunctionContext, memoryCache)
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
    where TQueryFunctionContext : IQueryFunctionContext
    where T : class
    {
        private Func<TFilter, TQueryFunctionContext, Task<T>>? _QueryFunc;
        private readonly TQueryFunctionContext _QueryFunctionContext;
        private readonly IMemoryCache _MemoryCache;
        private bool _CachingEnabled = true;

        public T? Data { get; protected set; }

        public Query(TQueryFunctionContext queryFunctionContext, IMemoryCache memoryCache)
        {
            _QueryFunctionContext = queryFunctionContext ?? throw new ArgumentNullException(nameof(queryFunctionContext));
            _MemoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            
            // TODO: temp disable caching for debugging. pls remove this!
            _CachingEnabled = false;
        }

        public void SetQueryFunction(Func<TFilter, TQueryFunctionContext, Task<T>> queryFunction)
        {
            _QueryFunc = queryFunction ?? throw new ArgumentNullException(nameof(queryFunction));       
        }
        
        public async Task<T?> Execute(TFilter filter)
        {
            ThrowIfQueryFunctionIsNull();
            
            Error = null;
            Status = QueryStatus.Pending;

            var cacheKey = GenerateCacheKey(filter);

            // check if the data is already cached, and if so, use it
            if (_MemoryCache.Get(cacheKey) is T cachedData)
            {
                Data = cachedData;
                Status = QueryStatus.Success;
                return Data;
            }

            // if not cached, execute the query function
            try
            {
                Data = await _QueryFunc(filter, _QueryFunctionContext);

                // check for errors raised by the function itself.
                // TODO: currently using exceptions for flow logic, but should be 
                //       replaced with a more structured error handling approach
                if (_QueryFunctionContext.ErrorMessage.Length > 0)
                {
                    throw new Exception(_QueryFunctionContext.ErrorMessage);
                }
            }
            catch (Exception e)
            {
                Status = QueryStatus.Error;
                Error = e;
                return null;
            }
            
            // cache the result
            if (_CachingEnabled)
            {
                _MemoryCache.Set(cacheKey, Data);
            }

            Status = QueryStatus.Success;
            return Data;
        }

        private string GenerateCacheKey(TFilter filter)
        {
            // generate cache-key
            var dataType = typeof(T);
            var filterType = typeof(TFilter);
            var cacheKey = $"{dataType.Namespace}-{dataType.Name}-{filterType.Name}-{filter}";
            return cacheKey;
        }
        
        [MemberNotNull("_QueryFunc")]
        private void ThrowIfQueryFunctionIsNull()
        {
            if (_QueryFunc == null)
            {
                throw new InvalidOperationException("Query.SetQueryFunction() must be called before executing the query.");
            }
        }
    }