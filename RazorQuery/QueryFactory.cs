using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace RazorQuery;

public static class QueryFactory
{
    private static IServiceProvider? _ServiceProvider;
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        _ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public static Query<T, TFilter> Create<T, TFilter>(Func<TFilter, DefaultQueryFunctionContext, Task<T>> queryFunction) 
        where T : class
    {
        ThrowIfServiceProviderIsNull();
        
        var ctx = _ServiceProvider.GetRequiredService<DefaultQueryFunctionContext>();
        var cache = _ServiceProvider.GetRequiredService<IMemoryCache>();
        
        var q = new Query<T, TFilter>(ctx, cache);
        q.SetQueryFunction(queryFunction);
        return q;
    }

    public static Query<T, TFilter, TQueryFunctionContext> Create<T, TFilter, TQueryFunctionContext>(Func<TFilter, TQueryFunctionContext, Task<T>> queryFunction)
        where TQueryFunctionContext : IQueryFunctionContext 
        where T : class
    {
        ThrowIfServiceProviderIsNull();

        var ctx = _ServiceProvider.GetRequiredService<TQueryFunctionContext>();
        var cache = _ServiceProvider.GetRequiredService<IMemoryCache>();

        var q = new Query<T, TFilter, TQueryFunctionContext>(ctx, cache);
        q.SetQueryFunction(queryFunction);
        return q;
    }

    [MemberNotNull(nameof(_ServiceProvider))]
    private static void ThrowIfServiceProviderIsNull()
    {
        if (_ServiceProvider == null)
        {
            throw new InvalidOperationException("QueryFactory.SetServiceProvider must be called before creating queries.");
        }
    }
}