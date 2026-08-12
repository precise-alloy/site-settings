using System.Linq;
using EPiServer.Applications;
using EPiServer.DataAccess;
using EPiServer.Security;
using Microsoft.Extensions.Logging;
using TuyenPham.SiteSettings.Models;

namespace TuyenPham.SiteSettings.Services;

public partial class SettingsService
{
    /// <summary>
    /// Resolves the current site identifier from the CMS application context.
    /// </summary>
    /// <returns>The site name, or <c>null</c> if the site cannot be resolved.</returns>
    private string? ResolveSiteId()
    {
        return _applicationResolver.GetByContext()?.Name;
    }

    /// <inheritdoc />
    public void SiteCreated(Application application)
    {
        if (GlobalSettingsRoot is not { } root)
        {
            _logger.LogWarning("[Settings] Site {siteName} was created before the settings root was initialized", application.Name);
            return;
        }

        if (!_contentRepository
                .GetChildren<SettingsFolder>(root)
                .Any(x => IsFolderForSite(x, application.Name)))
        {
            EnsureSettings(CreateSiteFolder(application), application.Name);
        }
    }

    /// <inheritdoc />
    public void SiteDeleted(Application application)
    {
        if (GlobalSettingsRoot is not { } root)
        {
            _logger.LogWarning("[Settings] Site {siteName} was deleted before the settings root was initialized", application.Name);
            return;
        }

        var folder = _contentRepository
            .GetChildren<SettingsFolder>(root)
            .FirstOrDefault(x => IsFolderForSite(x, application.Name));

        if (folder == null)
        {
            return;
        }

        _contentRepository.Delete(folder.ContentLink, true, AccessLevel.NoAccess);
        ClearCache();
    }

    /// <inheritdoc />
    public void SiteUpdated(Application application, Application previousApplication)
    {
        if (GlobalSettingsRoot is not { } settingsRoot)
        {
            _logger.LogWarning("[Settings] Site {siteName} was updated before the settings root was initialized", application.Name);
            return;
        }

        if (string.Equals(application.Name, previousApplication.Name, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (_contentRepository
                .GetChildren<SettingsFolder>(settingsRoot)
                .FirstOrDefault(x => IsFolderForSite(x, previousApplication.Name)) is SettingsFolder currentSettingsFolder)
        {
            var cloneFolder = (SettingsFolder)currentSettingsFolder.CreateWritableClone();
            if (string.Equals(currentSettingsFolder.Name, previousApplication.Name, StringComparison.OrdinalIgnoreCase))
            {
                cloneFolder.Name = application.Name;
            }
            cloneFolder.SiteId = application.Name;
            _contentRepository.Save(cloneFolder, SaveAction.Publish, AccessLevel.NoAccess);
        }
        else
        {
            EnsureSettings(CreateSiteFolder(application), application.Name);
        }

        ClearCache();
    }
}
