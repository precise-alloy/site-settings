namespace TuyenPham.SiteSettings.Models;

/// <summary>
/// Base class for all site settings content types. Extend this class to create custom settings
/// that can be managed per site in the Optimizely CMS admin interface.
/// </summary>
public abstract class SettingsBase
    : StandardContentBase, IReadOnly<IContent>
{
    /// <summary>
    /// Creates a writable copy of the current settings content instance.
    /// </summary>
    /// <returns>A deep-copy writable clone of this settings content as <see cref="IContent"/>.</returns>
    public new IContent CreateWritableClone()
    {
        return (base.CreateWritableClone() as IContent)!;
    }
}