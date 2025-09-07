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
    {
        ThrowIfServiceProviderIsNull();

        var ctx = _ServiceProvider.GetRequiredService<DefaultQueryFunctionContext>();

        return new Query<T, TFilter>(queryFunction, ctx);
    }

    public static Query<T, TFilter, TQueryFunctionContext> Create<T, TFilter, TQueryFunctionContext>(Func<TFilter, TQueryFunctionContext, Task<T>> queryFunction)
        where TQueryFunctionContext : IQueryFunctionContext
    {
        ThrowIfServiceProviderIsNull();

        var ctx = _ServiceProvider.GetRequiredService<TQueryFunctionContext>();

        return new Query<T, TFilter, TQueryFunctionContext>(queryFunction, ctx);
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