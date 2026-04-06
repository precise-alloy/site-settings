namespace TuyenPham.SiteSettings.Models;

/// <summary>
/// Represents a folder in the content tree used to organize site settings.
/// Each site has its own settings folder under the global settings root.
/// </summary>
[ContentType(
    DisplayName = "Settings Folder",
    GUID = "c709627f-ca9f-4c77-b0fb-8563287ebd93")]
[AvailableContentTypes(Include = [typeof(SettingsBase), typeof(SettingsFolder)])]
public class SettingsFolder
    : ContentFolder
{
    /// <summary>
    /// The registered name of the settings root content node.
    /// </summary>
    public const string SettingsRootName = "SettingsRoot";

    /// <summary>
    /// The unique identifier for the settings root content node.
    /// </summary>
    public static readonly Guid SettingsRootGuid = new("79611ee5-7ddd-4ac8-b00e-5e8e8d2a57ee");

    private Injected<LocalizationService> _localizationService;
    private static Injected<ContentRootService> _rootService;

    /// <summary>
    /// Gets the <see cref="ContentReference"/> to the global settings root.
    /// </summary>
    public static ContentReference SettingsRoot => GetSettingsRoot();

    /// <summary>
    /// Gets or sets the name of this settings folder. When this folder is the settings root,
    /// the localized "Site Settings" name is returned instead of the stored name.
    /// </summary>
    public override string Name
    {
        get => ContentLink.CompareToIgnoreWorkID(SettingsRoot)
            ? _localizationService.Service.GetString("/contentrepositories/globalsettings/Name", "Site Settings")
            : base.Name;

        set => base.Name = value;
    }

    /// <summary>
    /// Resolves the <see cref="ContentReference"/> for the settings root from the <see cref="ContentRootService"/>.
    /// </summary>
    /// <returns>The <see cref="ContentReference"/> to the settings root.</returns>
    private static ContentReference GetSettingsRoot() => _rootService.Service.Get(SettingsRootName);
}