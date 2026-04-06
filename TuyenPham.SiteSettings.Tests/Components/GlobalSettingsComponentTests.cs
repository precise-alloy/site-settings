using EPiServer.Shell;
using EPiServer.Shell.ViewComposition;
using TuyenPham.SiteSettings.Components;
using TuyenPham.SiteSettings.Descriptors;

namespace TuyenPham.SiteSettings.Tests.Components;

public class GlobalSettingsComponentTests
{
    [Fact]
    public void Constructor_SetsLanguagePath()
    {
        var component = new GlobalSettingsComponent();

        Assert.Equal("/episerver/cms/components/globalsettings", component.LanguagePath);
    }

    [Fact]
    public void Constructor_SetsTitle()
    {
        var component = new GlobalSettingsComponent();

        Assert.Equal("Site Settings", component.Title);
    }

    [Fact]
    public void Constructor_SetsSortOrderTo1000()
    {
        var component = new GlobalSettingsComponent();

        Assert.Equal(1000, component.SortOrder);
    }

    [Fact]
    public void Constructor_SetsPlugInAreasToAssetsDefaultGroup()
    {
        var component = new GlobalSettingsComponent();

        Assert.Single(component.PlugInAreas);
        Assert.Contains(PlugInArea.AssetsDefaultGroup, component.PlugInAreas);
    }

    [Fact]
    public void Constructor_SetsRepositoryKeySetting()
    {
        var component = new GlobalSettingsComponent();

        var repositoryKeySetting = component.Settings
            .FirstOrDefault(s => s.Key == "repositoryKey");

        Assert.Equal(GlobalSettingsRepositoryDescriptor.RepositoryKey, repositoryKeySetting.Value);
    }

    [Fact]
    public void Class_HasComponentAttribute()
    {
        var attribute = typeof(GlobalSettingsComponent)
            .GetCustomAttributes(typeof(ComponentAttribute), false)
            .Cast<ComponentAttribute>()
            .SingleOrDefault();

        Assert.NotNull(attribute);
    }

    [Fact]
    public void Class_IsSealed()
    {
        Assert.True(typeof(GlobalSettingsComponent).IsSealed);
    }

    [Fact]
    public void Class_InheritsFromComponentDefinitionBase()
    {
        Assert.True(typeof(ComponentDefinitionBase).IsAssignableFrom(typeof(GlobalSettingsComponent)));
    }
}
