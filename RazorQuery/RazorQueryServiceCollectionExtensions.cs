using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace RazorQuery;

/// <summary>
/// Provides extension methods for registering and configuring RazorQuery services in an application.
/// </summary>
public static class RazorQueryServiceCollectionExtensions
{
    /// <summary>
    /// Adds the RazorQuery services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    public static IServiceCollection AddRazorQuery(this IServiceCollection services)
    {
        // RazorQuery requires a HttpClient to function properly, so we add it here.
        services.AddHttpClient();

        // Register RazorQuery services
        services.AddTransient<DefaultQueryFunctionContext>();

        return services;
    }

    /// <summary>
    /// Configures the RazorQuery services to work with the provided host.
    /// </summary>
    public static WebAssemblyHost UseRazorQuery(this WebAssemblyHost host)
    {
        QueryFactory.SetServiceProvider(host.Services);
        
        return host;
    }
}