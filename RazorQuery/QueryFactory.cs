using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace RazorQuery;

public static class QueryFactory
{
    private static IServiceProvider? _ServiceProvider;
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        _ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public static Query<T, TFilter> Create<T, TFilter>(Func<TFilter, QueryFunctionContext, Task<T>> queryFunction) //where T : new()
    {
        ThrowIfQueryFunctionIsNull(queryFunction);
        ThrowIfServiceProviderIsNull();
        
        var ctx = _ServiceProvider!.GetRequiredService<QueryFunctionContext>();
        
        return new Query<T, TFilter>(queryFunction, ctx);
    }

    private static void ThrowIfQueryFunctionIsNull<T, TFilter>(Func<TFilter, QueryFunctionContext, Task<T>> queryFunction) //where T : new()
    {
        if (queryFunction == null)
        {
            throw new ArgumentNullException(nameof(queryFunction), "Query function cannot be null.");
        }
    }
    
    private static void ThrowIfServiceProviderIsNull()
    {
        if (_ServiceProvider == null)
        {
            throw new InvalidOperationException("QueryFactory.SetServiceProvider must be called before creating queries.");       
        }
    }
}