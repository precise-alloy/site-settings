using System.Linq;
using EPiServer.Applications;
using EPiServer.DataAccess;
using EPiServer.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TuyenPham.SiteSettings.Models;

namespace TuyenPham.SiteSettings.Services;

public partial class SiteSettingsService
{
    /// <summary>
    /// Resolves the current site identifier from the HTTP context using <see cref="EPiServer.Applications.IApplicationResolver"/>.
    /// </summary>
    /// <returns>The site name, or <c>null</c> if the site cannot be resolved.</returns>
    private string? ResolveSiteId()
    {
        var request = _httpContextAccessor.HttpContext?.RequestServices
            .GetRequiredService<IApplicationResolver>();

        return request?.GetByContext()?.Name;
    }

    /// <inheritdoc />
    public void SiteCreated(
        object? sender,
        ApplicationCreatedEvent e)
    {
        if (sender == null)
        {
            return;
        }

        if (!_contentRepository
                .GetChildren<SettingsFolder>(GlobalSettingsRoot)
                .Any(x => x.Name.Equals(e.Application.Name, StringComparison.InvariantCultureIgnoreCase)))
        {
            CreateSiteFolder(e.Application);
        }
    }

    /// <inheritdoc />
    public void SiteDeleted(
        object? sender,
        ApplicationDeletedEvent e)
    {
        if (sender == null)
        {
            return;
        }

        var folder = _contentRepository
            .GetChildren<SettingsFolder>(GlobalSettingsRoot)
            .FirstOrDefault(x => x.Name.Equals(e.Application.Name, StringComparison.InvariantCultureIgnoreCase));

        if (folder == null)
        {
            return;
        }

        _contentRepository.Delete(folder.ContentLink, true, AccessLevel.NoAccess);
        ClearCache();
    }

    /// <inheritdoc />
    public void SiteUpdated(
        object? sender,
        ApplicationUpdatedEvent e)
    {
        if (sender == null)
        {
            return;
        }

        if (e is not ApplicationUpdatedEvent updatedArgs)
        {
            _logger.LogError("SiteUpdated fail as SiteDefinitionEventArgs is not of type SiteDefinitionUpdatedEventArgs");
            return;
        }

        var prevSite = updatedArgs.PreviousApplication;
        var updatedSite = updatedArgs.Application;
        var settingsRoot = GlobalSettingsRoot;

        if (_contentRepository
                .GetChildren<IContent>(settingsRoot)
                .FirstOrDefault(x => x.Name.Equals(prevSite.Name, StringComparison.InvariantCultureIgnoreCase)) is ContentFolder currentSettingsFolder)
        {
            var cloneFolder = currentSettingsFolder.CreateWritableClone();
            cloneFolder.Name = updatedSite.Name;
            _contentRepository.Save(cloneFolder, SaveAction.Publish, AccessLevel.NoAccess);
        }
        else
        {
            CreateSiteFolder(e.Application);
        }
    }
}
