using EPiServer.Applications;
using TuyenPham.SiteSettings.Models;

namespace TuyenPham.SiteSettings.Services;

/// <summary>
/// Provides site-level settings management including initialization, retrieval, and site lifecycle handling.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets the <see cref="ContentReference"/> to the global settings root folder.
    /// </summary>
    ContentReference? GlobalSettingsRoot { get; }

    /// <summary>
    /// Initializes the settings infrastructure by registering content roots and subscribing to content events.
    /// </summary>
    void InitializeSettings();

    /// <summary>
    /// Retrieves the settings of the specified type for a given site and language.
    /// </summary>
    /// <typeparam name="T">The settings content type, which must derive from <see cref="SettingsBase"/>.</typeparam>
    /// <param name="siteId">The site identifier. If <c>null</c>, the current CMS application is used. Values are resolved case-insensitively and normalized to the CMS application name; blank or unknown values return <c>null</c>.</param>
    /// <param name="language">The language branch. If <c>null</c>, the preferred culture is used.</param>
    /// <returns>The settings instance, or <c>null</c> if not found.</returns>
    T? GetSiteSettings<T>(string? siteId = null, string? language = null) where T : SettingsBase;

    /// <summary>
    /// Removes local content-event subscriptions during CMS shutdown.
    /// </summary>
    void UninitializeSettings();

    /// <summary>
    /// Applies a site creation event.
    /// </summary>
    /// <param name="application">The newly created CMS application.</param>
    void SiteCreated(Application application);

    /// <summary>
    /// Applies a site deletion event by permanently removing the corresponding settings folder.
    /// </summary>
    /// <param name="application">The deleted CMS application.</param>
    void SiteDeleted(Application application);

    /// <summary>
    /// Applies a site rename or update event.
    /// </summary>
    /// <param name="application">The current CMS application.</param>
    /// <param name="previousApplication">The application state before the update.</param>
    void SiteUpdated(Application application, Application previousApplication);

}