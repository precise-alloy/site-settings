using Microsoft.Extensions.DependencyInjection;
using TuyenPham.SiteSettings.DependencyInjection;
using TuyenPham.SiteSettings.Services;

namespace TuyenPham.SiteSettings.Tests.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSiteSettings_RegistersISettingsServiceAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddSiteSettings();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ISettingsService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddSiteSettings_RegistersSettingsServiceAsImplementation()
    {
        var services = new ServiceCollection();

        services.AddSiteSettings();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ISettingsService));
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(SettingsService), descriptor.ImplementationType);
    }

    [Fact]
    public void AddSiteSettings_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddSiteSettings();

        Assert.Same(services, result);
    }

    [Fact]
    public void AddSiteSettings_CalledMultipleTimes_RegistersMultipleDescriptors()
    {
        var services = new ServiceCollection();

        services.AddSiteSettings();
        services.AddSiteSettings();

        var descriptors = services.Where(d => d.ServiceType == typeof(ISettingsService)).ToList();
        Assert.Equal(2, descriptors.Count);
    }
}
