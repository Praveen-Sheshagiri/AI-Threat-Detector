using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ThreatDetector.SDK.Extensions;

/// <summary>
/// Extension methods for dependency injection configuration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Threat Detector SDK services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddThreatDetector(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddThreatDetector(configuration.GetSection(ThreatDetectorOptions.SectionName));
    }

    /// <summary>
    /// Adds Threat Detector SDK services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configurationSection">Configuration section for threat detector options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddThreatDetector(
        this IServiceCollection services,
        IConfigurationSection configurationSection)
    {
        services.Configure<ThreatDetectorOptions>(configurationSection);
        
        services.AddHttpClient<IThreatDetectorClient, ThreatDetectorClient>((serviceProvider, httpClient) =>
        {
            var options = configurationSection.Get<ThreatDetectorOptions>() ?? new ThreatDetectorOptions();
            httpClient.BaseAddress = new Uri(options.ApiBaseUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds);
            
            if (!string.IsNullOrEmpty(options.ApiKey))
            {
                httpClient.DefaultRequestHeaders.Add("X-API-Key", options.ApiKey);
            }
            
            httpClient.DefaultRequestHeaders.Add("X-Application-Id", options.ApplicationId);
        });

        services.AddScoped<IThreatDetectorClient, ThreatDetectorClient>();
        services.AddSingleton<IThreatDetectorMiddleware, ThreatDetectorMiddleware>();
        
        return services;
    }

    /// <summary>
    /// Adds Threat Detector SDK services with custom options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddThreatDetector(
        this IServiceCollection services,
        Action<ThreatDetectorOptions> configureOptions)
    {
        services.Configure(configureOptions);
        
        services.AddHttpClient<IThreatDetectorClient, ThreatDetectorClient>();
        services.AddScoped<IThreatDetectorClient, ThreatDetectorClient>();
        services.AddSingleton<IThreatDetectorMiddleware, ThreatDetectorMiddleware>();
        
        return services;
    }
}
