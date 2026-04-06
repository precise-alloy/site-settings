using EPiServer.Core;
using EPiServer.DataAnnotations;
using TuyenPham.SiteSettings.Models;

namespace TuyenPham.SiteSettings.Tests.Models;

public class SettingsBaseTests
{
    [Fact]
    public void Class_IsAbstract()
    {
        Assert.True(typeof(SettingsBase).IsAbstract);
    }

    [Fact]
    public void Class_InheritsFromStandardContentBase()
    {
        Assert.True(typeof(StandardContentBase).IsAssignableFrom(typeof(SettingsBase)));
    }

    [Fact]
    public void Class_ImplementsIReadOnlyOfIContent()
    {
        Assert.True(typeof(EPiServer.Data.Entity.IReadOnly<IContent>).IsAssignableFrom(typeof(SettingsBase)));
    }
}
