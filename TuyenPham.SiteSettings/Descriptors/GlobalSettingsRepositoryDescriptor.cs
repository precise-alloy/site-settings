using System.Collections.Generic;
using EPiServer.Cms.Shell.UI.CompositeViews.Internal;
using EPiServer.Shell;
using TuyenPham.SiteSettings.Models;
using TuyenPham.SiteSettings.Providers;
using TuyenPham.SiteSettings.Services;

namespace TuyenPham.SiteSettings.Descriptors;

/// <summary>
/// Describes the global settings content repository for the Optimizely CMS navigation tree.
/// Provides metadata such as root references, contained types, and search area configuration.
/// </summary>
[ServiceConfiguration(typeof(IContentRepositoryDescriptor))]
public class GlobalSettingsRepositoryDescriptor : ContentRepositoryDescriptorBase
{
    /// <summary>
    /// Gets the unique repository key used to identify the global settings repository.
    /// </summary>
    public static string RepositoryKey => "globalsettings";

    /// <inheritdoc />
    public override IEnumerable<Type> ContainedTypes => [typeof(SettingsBase), typeof(SettingsFolder)];

    /// <inheritdoc />
    public override IEnumerable<Type> CreatableTypes => [typeof(SettingsBase), typeof(SettingsFolder)];

    /// <inheritdoc />
    public override string CustomNavigationWidget => "epi-cms/component/ContentNavigationTree";

    /// <inheritdoc />
    public override string CustomSelectTitle => LocalizationService.Current.GetString($"/contentRepositories/{RepositoryKey}/customSelectTitle", "Settings");

    /// <inheritdoc />
    public override string Key => RepositoryKey;

    /// <inheritdoc />
    public override IEnumerable<Type> MainNavigationTypes => [typeof(SettingsBase), typeof(SettingsFolder)];

    /// <inheritdoc />
    public override IEnumerable<string> MainViews => [HomeView.ViewName];

    /// <inheritdoc />
    public override string Name => LocalizationService.Current.GetString($"/contentRepositories/{RepositoryKey}/name", "Site Settings");

    /// <inheritdoc />
    public override IEnumerable<ContentReference> Roots
    {
        get
        {
            if (_settings.Service?.GlobalSettingsRoot is { } root
                && !ContentReference.IsNullOrEmpty(root))
            {
                return [root];
            }

            return [ContentReference.EmptyReference];
        }
    }

    /// <inheritdoc />
    public override string SearchArea => GlobalSettingsSearchProvider.SearchArea;

    /// <inheritdoc />
    public override int SortOrder => 1000;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private readonly Injected<ISettingsService> _settings;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
}
