namespace TuyenPham.SiteSettings.Models;

/// <summary>
/// Attribute used to identify content types as site settings. Automatically sets the group name
/// to <see cref="SystemTabNames.Settings"/> and clears the description.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class SettingsContentTypeAttribute
    : ContentTypeAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsContentTypeAttribute"/> class
    /// with an empty description and the settings group name.
    /// </summary>
    public SettingsContentTypeAttribute(
        string groupName = SystemTabNames.Settings)
    {
        Description = string.Empty;
        GroupName = groupName;
    }
}