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
    private EventHandler? _initCompleteHandler;

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
        ArgumentNullException.ThrowIfNull(context);

        _initCompleteHandler = (_, _) =>
        {
            context.Services
                .GetInstance<ISettingsService>()
                .InitializeSettings();
        };
        context.InitComplete += _initCompleteHandler;
    }

    /// <summary>
    /// Removes initialization and content-event subscriptions during CMS shutdown.
    /// </summary>
    /// <param name="context">The initialization engine.</param>
    void IInitializableModule.Uninitialize(InitializationEngine context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (_initCompleteHandler != null)
        {
            context.InitComplete -= _initCompleteHandler;
            _initCompleteHandler = null;
        }

        context.Services
            .GetInstance<ISettingsService>()
            .UninitializeSettings();
    }

}