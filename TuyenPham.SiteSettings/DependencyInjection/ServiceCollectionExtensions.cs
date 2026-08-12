using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using EPiServer.Applications;
using EPiServer.DependencyInjection;
using TuyenPham.SiteSettings.Infrastructure;
using TuyenPham.SiteSettings.Services;

namespace TuyenPham.SiteSettings.DependencyInjection;

/// <summary>
/// Extension methods for registering content area item options in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="ISettingsService"/> as a singleton in the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the site settings service to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddSiteSettings(
        this IServiceCollection services)
    {
        services.TryAddSingleton<ISettingsService, SettingsService>();
        services.AddCmsEventSubscriber<ApplicationCreatedEvent, SettingsEventSubscriber>();
        services.AddCmsEventSubscriber<ApplicationDeletedEvent, SettingsEventSubscriber>();
        services.AddCmsEventSubscriber<ApplicationUpdatedEvent, SettingsEventSubscriber>();

        return services;
    }
}
