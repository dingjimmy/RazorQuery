using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace RazorQuery;

public class QueryFactory
{
    private readonly IServiceProvider _ServiceProvider;

    public QueryFactory(IServiceProvider serviceProvider)
    {
        _ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public Query<T, TFilter> Create<T, TFilter>(Func<TFilter, QueryFunctionContext, Task<T>> queryFunction) where T : new()
    {             
        if (queryFunction == null)
        {
            throw new ArgumentNullException(nameof(queryFunction), "Query function cannot be null.");
        }

        var ctx = _ServiceProvider.GetRequiredService<QueryFunctionContext>();

        return new Query<T, TFilter>(queryFunction, ctx);
    }
}