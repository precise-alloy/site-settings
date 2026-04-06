using EPiServer.Core;
using EPiServer.DataAnnotations;
using TuyenPham.SiteSettings.Models;

namespace TuyenPham.SiteSettings.Tests.Models;

public class SettingsFolderTests
{
    [Fact]
    public void SettingsRootName_IsExpectedValue()
    {
        Assert.Equal("SettingsRoot", SettingsFolder.SettingsRootName);
    }

    [Fact]
    public void SettingsRootGuid_IsExpectedValue()
    {
        Assert.Equal(new Guid("79611ee5-7ddd-4ac8-b00e-5e8e8d2a57ee"), SettingsFolder.SettingsRootGuid);
    }

    [Fact]
    public void Class_InheritsFromContentFolder()
    {
        Assert.True(typeof(ContentFolder).IsAssignableFrom(typeof(SettingsFolder)));
    }

    [Fact]
    public void Class_HasContentTypeAttribute()
    {
        var attribute = typeof(SettingsFolder)
            .GetCustomAttributes(typeof(ContentTypeAttribute), false)
            .Cast<ContentTypeAttribute>()
            .Single();

        Assert.Equal("Settings Folder", attribute.DisplayName);
        Assert.Equal("c709627f-ca9f-4c77-b0fb-8563287ebd93", attribute.GetGUID().ToString());
    }

    [Fact]
    public void Class_HasAvailableContentTypesAttribute()
    {
        var attribute = typeof(SettingsFolder)
            .GetCustomAttributes(typeof(AvailableContentTypesAttribute), false)
            .Cast<AvailableContentTypesAttribute>()
            .Single();

        Assert.Contains(typeof(SettingsBase), attribute.Include);
        Assert.Contains(typeof(SettingsFolder), attribute.Include);
    }
}
