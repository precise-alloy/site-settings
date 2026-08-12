using TuyenPham.SiteSettings.Models;

namespace TuyenPham.SiteSettings.Services;

public partial class SettingsService
{
    /// <summary>
    /// Clears settings caches when fallback configuration for the root page or settings content changes.
    /// </summary>
    /// <param name="contentLink">The content whose language settings changed.</param>
    internal void ContentLanguageSettingsChanged(ContentReference contentLink)
    {
        if (contentLink == ContentReference.RootPage
            || _contentRepository.TryGet(contentLink, out SettingsBase _))
        {
            ClearCache();
        }
    }

    private void ContentLanguageSettingSavedOrDeleted(
        object? sender,
        ContentLanguageSettingEventArgs? eventArgs)
    {
        if (eventArgs != null)
        {
            ContentLanguageSettingsChanged(eventArgs.ContentLink);
        }
    }
}
