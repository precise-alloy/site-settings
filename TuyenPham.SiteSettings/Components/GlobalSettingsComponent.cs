using EPiServer.Shell;
using EPiServer.Shell.ViewComposition;
using TuyenPham.SiteSettings.Descriptors;

namespace TuyenPham.SiteSettings.Components;

/// <summary>
/// Defines the "Site Settings" navigation component in the Optimizely CMS assets pane,
/// powered by the global settings content repository.
/// </summary>
[Component]
// ReSharper disable once UnusedMember.Global
public sealed class GlobalSettingsComponent
    : ComponentDefinitionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalSettingsComponent"/> class,
    /// configuring it with the global settings repository key, localization path, and sort order.
    /// </summary>
    public GlobalSettingsComponent()
        : base("epi-cms/component/MainNavigationComponent")
    {
        var repositoryKeySeting = new Setting(
            "repositoryKey",
            GlobalSettingsRepositoryDescriptor.RepositoryKey);

        LanguagePath = "/episerver/cms/components/globalsettings";
        Title = "Site Settings";
        SortOrder = 1000;
        PlugInAreas = [PlugInArea.AssetsDefaultGroup];
        Settings.Add(repositoryKeySeting);
    }
}

