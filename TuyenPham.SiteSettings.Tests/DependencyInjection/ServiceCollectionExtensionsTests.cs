using Microsoft.Extensions.DependencyInjection;
using EPiServer.Applications;
using EPiServer.Events;
using TuyenPham.SiteSettings.Infrastructure;
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
    public void AddSiteSettings_CalledMultipleTimes_RegistersOneService()
    {
        var services = new ServiceCollection();

        services.AddSiteSettings();
        services.AddSiteSettings();

        var descriptors = services.Where(d => d.ServiceType == typeof(ISettingsService)).ToList();
        Assert.Single(descriptors);
    }

    [Theory]
    [InlineData(typeof(ApplicationCreatedEvent))]
    [InlineData(typeof(ApplicationDeletedEvent))]
    [InlineData(typeof(ApplicationUpdatedEvent))]
    public void AddSiteSettings_RegistersLifecycleEventSubscriber(Type eventType)
    {
        var services = new ServiceCollection();

        services.AddSiteSettings();

        var subscriberType = typeof(IEventSubscriber<>).MakeGenericType(eventType);
        var descriptor = Assert.Single(services, x => x.ServiceType == subscriberType);
        Assert.Equal(typeof(SettingsEventSubscriber), descriptor.ImplementationType);
    }
}
