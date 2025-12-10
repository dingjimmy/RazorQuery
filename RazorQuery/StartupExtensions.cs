using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace RazorQuery;

/// <summary>
/// Provides extension methods for registering and configuring RazorQuery services in an application.
/// </summary>
public static class StartupExtensions
{
    /// <summary>
    /// Adds the RazorQuery services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    public static IServiceCollection AddRazorQuery(this IServiceCollection services)
    {
        // RazorQuery requires a HttpClient to function properly, so we add it here.
        services.AddHttpClient();
        
        // Register cache as 'scoped' to ensure there is only one cache per user session
        // (i.e. one 'per browser-context' in blazor wasm, and 'one per circuit' in blazor server) 
        services.AddScoped<IMemoryCache, MemoryCache>(); 
        
        // Register RazorQuery services
        services.AddTransient<DefaultQueryFunctionContext>();

        return services;
    }

    /// <summary>
    /// Configures RazorQuery to work on a Blazor WebAssembly host.
    /// </summary>
    public static WebAssemblyHost UseRazorQueryWasm(this WebAssemblyHost host)
    {
        QueryFactory.SetServiceProvider(host.Services);
        
        return host;
    }
}