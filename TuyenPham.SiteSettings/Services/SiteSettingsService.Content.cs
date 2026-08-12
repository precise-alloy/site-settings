using EPiServer;
using EPiServer.Cms.Shell;
using TuyenPham.SiteSettings.Models;

namespace TuyenPham.SiteSettings.Services;

public partial class SettingsService
{
    /// <summary>
    /// Handles the content saved event by evicting the local draft cache entry for the saved settings.
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

        if (TryGetSiteId(settings.ParentLink, out var siteId))
        {
            _cacheManager.RemoveLocal(CreateCacheKey(siteId, settings.GetOriginalType(), true));
            return;
        }

        // A missing parent prevents targeted invalidation; clear all entries rather than serving a stale draft.
        ClearCache();
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

        InvalidateSettingsCache(settings);
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

        InvalidateSettingsCache(settings);
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

    private void InvalidateSettingsCache(SettingsBase settings)
    {
        if (TryGetSiteId(settings.ParentLink, out var siteId))
        {
            RemoveCache(siteId, settings);
            return;
        }

        // A moved or deleted item may no longer have a readable parent. Broad invalidation avoids stale settings.
        ClearCache();
    }

    private bool TryGetSiteId(ContentReference folderLink, out string siteId)
    {
        siteId = string.Empty;
        if (!_contentRepository.TryGet(folderLink, out SettingsFolder? folder)
            || folder is null)
        {
            return false;
        }

        siteId = string.IsNullOrWhiteSpace(folder.SiteId) ? folder.Name : folder.SiteId;
        return !string.IsNullOrWhiteSpace(siteId)
            && _applicationRepository.Get(siteId) is not null;
    }
}
