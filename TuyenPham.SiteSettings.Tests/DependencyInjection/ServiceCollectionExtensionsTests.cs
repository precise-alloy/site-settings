using Microsoft.Extensions.DependencyInjection;
using TuyenPham.SiteSettings.DependencyInjection;
using TuyenPham.SiteSettings.Services;

namespace TuyenPham.SiteSettings.Tests.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSiteSettings_RegistersISiteSettingsServiceAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddSiteSettings();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ISiteSettingsService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddSiteSettings_RegistersSiteSettingsServiceAsImplementation()
    {
        var services = new ServiceCollection();

        services.AddSiteSettings();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ISiteSettingsService));
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(SiteSettingsService), descriptor.ImplementationType);
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

        var descriptors = services.Where(d => d.ServiceType == typeof(ISiteSettingsService)).ToList();
        Assert.Equal(2, descriptors.Count);
    }
}
