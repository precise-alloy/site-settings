using EPiServer.Framework;
using EPiServer.ServiceLocation;
using TuyenPham.SiteSettings.Infrastructure;

namespace TuyenPham.SiteSettings.Tests.Infrastructure;

public class SettingsInitializationTests
{
    [Fact]
    public void Class_ImplementsIConfigurableModule()
    {
        Assert.True(typeof(IConfigurableModule).IsAssignableFrom(typeof(SettingsInitialization)));
    }

    [Fact]
    public void Class_HasModuleDependencyAttribute()
    {
        var attribute = typeof(SettingsInitialization)
            .GetCustomAttributes(typeof(ModuleDependencyAttribute), false)
            .FirstOrDefault();

        Assert.NotNull(attribute);
    }

    [Fact]
    public void Class_DependsOnWebInitializationModule()
    {
        var attribute = (ModuleDependencyAttribute)typeof(SettingsInitialization)
            .GetCustomAttributes(typeof(ModuleDependencyAttribute), false)
            .First();

        Assert.Contains(typeof(EPiServer.Web.InitializationModule), attribute.Dependencies);
    }

    [Fact]
    public void ConfigureContainer_DoesNotThrow()
    {
        IConfigurableModule module = new SettingsInitialization();

        // ConfigureContainer with null context should not throw
        try
        {
            module.ConfigureContainer(null!);
        }
        catch
        {
            Assert.Fail("ConfigureContainer should not throw");
        }
    }

    [Fact]
    public void Uninitialize_DoesNotThrow()
    {
        IInitializableModule module = new SettingsInitialization();

        try
        {
            module.Uninitialize(null!);
        }
        catch
        {
            Assert.Fail("Uninitialize should not throw");
        }
    }
}
