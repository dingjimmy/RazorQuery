using Microsoft.AspNetCore.Builder;

namespace RazorQuery.BlazorServer;

/// <summary>
/// Provides extension methods for registering and configuring RazorQuery services in a "Blazor Server" application.
/// </summary>
public static class BlazorServerStartupExtensions
{
    /// <summary>
    /// Configures RazorQuery to work on a Blazor Server host.
    /// </summary>
    public static WebApplication UseRazorQueryServer(this WebApplication host)
    {
        QueryFactory.SetServiceProvider(host.Services);
    
        return host;
    }
}