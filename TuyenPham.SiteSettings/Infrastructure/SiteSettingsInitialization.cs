using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using TuyenPham.SiteSettings.Services;

namespace TuyenPham.SiteSettings.Infrastructure;

/// <summary>
/// Optimizely CMS initialization module that bootstraps the site settings infrastructure.
/// Registers the <see cref="ISettingsService"/> initialization on the <c>InitComplete</c> event.
/// </summary>
[ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
// ReSharper disable once UnusedMember.Global
public class SettingsInitialization
    : IConfigurableModule
{
    /// <summary>
    /// Configures the dependency injection container. No additional registrations are performed here.
    /// </summary>
    /// <param name="context">The service configuration context.</param>
    void IConfigurableModule.ConfigureContainer(ServiceConfigurationContext context)
    {
    }

    /// <summary>
    /// Subscribes to the <c>InitComplete</c> event to trigger <see cref="ISettingsService.InitializeSettings"/>
    /// once all CMS modules have finished initializing.
    /// </summary>
    /// <param name="context">The initialization engine providing access to the service locator.</param>
    void IInitializableModule.Initialize(InitializationEngine context)
    {
        context.InitComplete += (_, _) =>
        {
            context.Services
                .GetInstance<ISettingsService>()
                .InitializeSettings();
        };
    }

    /// <summary>
    /// Performs cleanup when the module is uninitialized. No cleanup actions are required.
    /// </summary>
    /// <param name="context">The initialization engine.</param>
    void IInitializableModule.Uninitialize(InitializationEngine context)
    {
    }

}