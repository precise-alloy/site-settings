using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using TuyenPham.SiteSettings.Models;

namespace TuyenPham.SiteSettings.Tests.Models;

public class SettingsContentTypeAttributeTests
{
    [Fact]
    public void Constructor_DefaultGroupName_SetsGroupNameToSettings()
    {
        var attribute = new SettingsContentTypeAttribute();

        Assert.Equal(SystemTabNames.Settings, attribute.GroupName);
    }

    [Fact]
    public void Constructor_DefaultGroupName_SetsDescriptionToEmpty()
    {
        var attribute = new SettingsContentTypeAttribute();

        Assert.Equal(string.Empty, attribute.Description);
    }

    [Fact]
    public void Constructor_CustomGroupName_SetsGroupName()
    {
        var attribute = new SettingsContentTypeAttribute(groupName: "CustomGroup");

        Assert.Equal("CustomGroup", attribute.GroupName);
    }

    [Fact]
    public void Constructor_CustomGroupName_StillSetsDescriptionToEmpty()
    {
        var attribute = new SettingsContentTypeAttribute(groupName: "CustomGroup");

        Assert.Equal(string.Empty, attribute.Description);
    }

    [Fact]
    public void Class_HasAttributeUsageForClassesOnly()
    {
        var attributeUsage = typeof(SettingsContentTypeAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .Single();

        Assert.Equal(AttributeTargets.Class, attributeUsage.ValidOn);
    }

    [Fact]
    public void Class_IsSealed()
    {
        Assert.True(typeof(SettingsContentTypeAttribute).IsSealed);
    }

    [Fact]
    public void Class_InheritsFromContentTypeAttribute()
    {
        Assert.True(typeof(ContentTypeAttribute).IsAssignableFrom(typeof(SettingsContentTypeAttribute)));
    }
}
