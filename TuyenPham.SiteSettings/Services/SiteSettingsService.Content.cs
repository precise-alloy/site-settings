using EPiServer;
using EPiServer.Cms.Shell;
using TuyenPham.SiteSettings.Models;

namespace TuyenPham.SiteSettings.Services;

public partial class SettingsService
{
    /// <summary>
    /// Handles the content saved event. Updates the draft cache for the saved settings content
    /// in the current site and language branch.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The content event arguments containing the saved content.</param>
    private void SavedContent(
        object? sender,
        ContentEventArgs? e)
    {
        if (sender == null)
        {
            return;
        }

        if (e?.Content is not SettingsBase settings)
        {
            return;
        }

        var id = ResolveSiteId();
        if (id is null)
        {
            return;
        }

        // Only need to update one cached draft settings when saving content (draft)
        // Only update locally since save happens very often and don't really need to update draft change on other server in CDN
        var type = settings.GetOriginalType();
        var settingsOfType = GetSettingFromCache(id, type, true);
        settingsOfType[settings.LanguageBranch()] = settings;
        InsertSettingToCache(id, type, true, settingsOfType);
    }

    /// <summary>
    /// Handles the content published event. Removes all cached entries for the published
    /// settings type and site, forcing a full cache repopulation on the next access.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The content event arguments containing the published content.</param>
    private void PublishedContent(
        object? sender,
        ContentEventArgs? e)
    {
        if (sender == null)
        {
            return;
        }

        if (e?.Content is not SettingsBase settings)
        {
            return;
        }

        var parent = _contentRepository.Get<IContent>(e.Content.ParentLink);
        var site = _applicationRepository.Get(parent.Name);

        var id = site?.Name;
        if (id is null)
        {
            return;
        }

        // Repopulate for all language: master lang update => other cached read from master will need to be changed,
        // fall back language change might lead to dependent language contents change
        // Also cached draft settings might need to be changed
        // So just to be safe, just repopulate everything, instead of checking for every possible cases above
        RemoveCache(id, settings);
    }

    /// <summary>
    /// Handles the content language deleted event. Removes cached entries for the
    /// settings type and site when a language branch is deleted.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The content event arguments containing the affected content.</param>
    private void DeletedContentLanguage(
        object? sender,
        ContentEventArgs? e)
    {
        if (sender == null)
        {
            return;
        }

        if (e?.Content is not SettingsBase settings)
        {
            return;
        }

        var parent = _contentRepository.Get<IContent>(e.Content.ParentLink);
        var site = _applicationRepository.Get(parent.Name);

        var id = site?.Name;
        if (id is null)
        {
            return;
        }

        RemoveCache(id, settings);
    }

    /// <summary>
    /// Handles the content moved event. Clears the entire settings cache when settings
    /// content is moved (e.g., to the wastebasket).
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The content event arguments containing the moved content.</param>
    private void MovedContent(
        object? sender,
        ContentEventArgs? e)
    {
        if (sender == null)
        {
            return;
        }

        if (e?.Content is SettingsBase)
        {
            ClearCache(); //apply to move to other folder to wastebasket
        }
    }
}
