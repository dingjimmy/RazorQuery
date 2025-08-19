using Microsoft.Extensions.DependencyInjection;

namespace RazorQuery;

public static class RazorQueryServiceCollectionExtensions
{
    public static IServiceCollection AddRazorQuery(this IServiceCollection services)
    {
        // RazorQuery requires a HttpClient to function properly, so we add it here.
        services.AddHttpClient();

        // Register RazorQuery services
        services.AddSingleton<QueryFactory>();
        services.AddTransient<QueryFunctionContext>();

        return services;
    }

}
