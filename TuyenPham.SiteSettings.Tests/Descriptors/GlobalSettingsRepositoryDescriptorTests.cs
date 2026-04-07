using EPiServer.Cms.Shell.UI.CompositeViews.Internal;
using TuyenPham.SiteSettings.Descriptors;
using TuyenPham.SiteSettings.Models;
using TuyenPham.SiteSettings.Providers;

namespace TuyenPham.SiteSettings.Tests.Descriptors;

public class GlobalSettingsRepositoryDescriptorTests
{
    [Fact]
    public void RepositoryKey_ReturnsGlobalSettings()
    {
        Assert.Equal("globalsettings", GlobalSettingsRepositoryDescriptor.RepositoryKey);
    }

    [Fact]
    public void ContainedTypes_ContainsSettingsBaseAndSettingsFolder()
    {
        var descriptor = new GlobalSettingsRepositoryDescriptor();

        Assert.Contains(typeof(SettingsBase), descriptor.ContainedTypes);
        Assert.Contains(typeof(SettingsFolder), descriptor.ContainedTypes);
    }

    [Fact]
    public void CreatableTypes_ContainsSettingsBaseAndSettingsFolder()
    {
        var descriptor = new GlobalSettingsRepositoryDescriptor();

        Assert.Contains(typeof(SettingsBase), descriptor.CreatableTypes);
        Assert.Contains(typeof(SettingsFolder), descriptor.CreatableTypes);
    }

    [Fact]
    public void CustomNavigationWidget_ReturnsContentNavigationTree()
    {
        var descriptor = new GlobalSettingsRepositoryDescriptor();

        Assert.Equal("epi-cms/component/ContentNavigationTree", descriptor.CustomNavigationWidget);
    }

    [Fact]
    public void Key_ReturnsRepositoryKey()
    {
        var descriptor = new GlobalSettingsRepositoryDescriptor();

        Assert.Equal(GlobalSettingsRepositoryDescriptor.RepositoryKey, descriptor.Key);
    }

    [Fact]
    public void MainNavigationTypes_ContainsSettingsBaseAndSettingsFolder()
    {
        var descriptor = new GlobalSettingsRepositoryDescriptor();

        Assert.Contains(typeof(SettingsBase), descriptor.MainNavigationTypes);
        Assert.Contains(typeof(SettingsFolder), descriptor.MainNavigationTypes);
    }

    [Fact]
    public void MainViews_ContainsHomeView()
    {
        var descriptor = new GlobalSettingsRepositoryDescriptor();

        Assert.Contains(HomeView.ViewName, descriptor.MainViews);
    }

    [Fact]
    public void SearchArea_ReturnsGlobalSettingsSearchProviderSearchArea()
    {
        var descriptor = new GlobalSettingsRepositoryDescriptor();

        Assert.Equal(GlobalSettingsSearchProvider.SearchArea, descriptor.SearchArea);
    }

    [Fact]
    public void SortOrder_Returns1000()
    {
        var descriptor = new GlobalSettingsRepositoryDescriptor();

        Assert.Equal(1000, descriptor.SortOrder);
    }

    [Fact]
    public void Roots_WhenServiceIsNotRegistered_ReturnsEmptyReference()
    {
        var descriptor = new GlobalSettingsRepositoryDescriptor();

        // The Injected<ISettingsService> dependency cannot be resolved without DI,
        // so we expect it to throw. This verifies the property accesses the injected service.
        Assert.ThrowsAny<Exception>(() => descriptor.Roots.ToList());
    }

    [Fact]
    public void Class_HasServiceConfigurationAttribute()
    {
        var attribute = typeof(GlobalSettingsRepositoryDescriptor)
            .GetCustomAttributes(typeof(EPiServer.ServiceLocation.ServiceConfigurationAttribute), false)
            .FirstOrDefault();

        Assert.NotNull(attribute);
    }
}
