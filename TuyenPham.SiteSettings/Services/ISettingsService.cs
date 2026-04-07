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
    /// <param name="siteId">The site identifier. If <c>null</c>, the current site is resolved from the HTTP context.</param>
    /// <param name="language">The language branch. If <c>null</c>, the preferred culture is used.</param>
    /// <returns>The settings instance, or <c>null</c> if not found.</returns>
    T? GetSiteSettings<T>(string? siteId = null, string? language = null) where T : SettingsBase;

    /// <summary>
    /// Handles the site created event by creating a settings folder and default settings for the new site.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event arguments containing the created application.</param>
    void SiteCreated(object? sender, ApplicationCreatedEvent e);

    /// <summary>
    /// Handles the site deleted event by removing the corresponding settings folder and clearing the cache.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event arguments containing the deleted application.</param>
    void SiteDeleted(object? sender, ApplicationDeletedEvent e);

    /// <summary>
    /// Handles the site updated event by renaming the settings folder or creating one if it doesn't exist.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event arguments containing the previous and updated application details.</param>
    void SiteUpdated(object? sender, ApplicationUpdatedEvent e);
}