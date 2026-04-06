using TuyenPham.SiteSettings.Models;

namespace TuyenPham.SiteSettings.Services;

public partial class SiteSettingsService
{
    /// <summary>
    /// Handles language setting saved or deleted events. Clears the entire settings cache when
    /// the root page or a settings content item has its language settings modified.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event arguments containing the affected content link.</param>
    private void ContentLanguageSettingSavedOrDeleted(
        object? sender,
        ContentLanguageSettingEventArgs? e)
    {
        if (sender == null
            || e == null)
        {
            return;
        }

        if (e.ContentLink == ContentReference.RootPage
            || _contentRepository.TryGet(e.ContentLink, out SettingsBase _))
        {
            ClearCache();
        }
    }
}
