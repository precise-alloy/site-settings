# TuyenPham.SiteSettings

A plugin for [Optimizely CMS](https://www.optimizely.com/products/content-management/) that adds per-site settings management directly in the CMS editor UI. Define strongly-typed settings content types and retrieve them in code with full multi-language and multi-site support.

## Features

- **Per-site settings** — each site gets its own settings folder, created automatically when a site is added and retained when its display name changes.
- **Strongly-typed** — define settings as C# classes with property editors, just like regular Optimizely content.
- **Multi-language** — settings respect language branches and fallback chains configured in the CMS.
- **Edit & published mode** — draft settings are available in edit mode, published settings in default mode.
- **CMS integration** — settings appear in the assets pane navigation tree and are searchable via the global search.
- **Caching** — settings are cached per site/type/language with synchronized invalidation on content and language changes.

## Requirements

- .NET 10+
- Optimizely CMS 13.1+ (`EPiServer.CMS.UI.Core >= 13.1.1`)

## Installation

Install from [Microsoft NuGet](https://www.nuget.org/packages/TuyenPham.SiteSettings) or [Optimizely Nuget](https://nuget.optimizely.com/packages/tuyenpham.sitesettings):

```shell wrap
dotnet add package TuyenPham.SiteSettings
```

Register the service in your `Startup.cs` or `Program.cs`:

```csharp wrap
using TuyenPham.SiteSettings.DependencyInjection;

services.AddSiteSettings();
```

The module initializes automatically via `IConfigurableModule`. No additional startup code is needed.

## Usage

### 1. Define a settings content type

Create a class that inherits from `SettingsBase` and decorate it with `[SettingsContentType]`:

```csharp
using TuyenPham.SiteSettings.Models;

[SettingsContentType(
    DisplayName = "General Settings",
    GUID = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx")]
public class GeneralSettings
    : SettingsBase
{
    [Display(
        Name = "Site Title",
        GroupName = SystemTabNames.Content,
        Order = 10)]
    public virtual string? SiteTitle { get; set; }

    [Display(
        Name = "Footer Text",
        GroupName = SystemTabNames.Content,
        Order = 20)]
    public virtual string? FooterText { get; set; }
}
```

A settings instance is automatically created for each site. Startup reconciliation also creates instances for settings types added after a site already exists.

### 2. Retrieve settings in code

Inject `ISettingsService` and call `GetSiteSettings<T>()`:

```csharp
using TuyenPham.SiteSettings.Services;

public class MyController(
    ISettingsService settingsService)
     : Controller
{
    public IActionResult Index()
    {
        var settings = settingsService
        .GetSiteSettings<GeneralSettings>();

        // settings?.SiteTitle,
        // settings?.FooterText,
        // etc.
        return View(settings);
    }
}
```

#### Parameters

| Parameter  | Type      | Default | Description                                                          |
| ---------- | --------- | ------- | -------------------------------------------------------------------- |
| `siteId`   | `string?` | `null`  | Site identifier. Resolved from the current HTTP context when `null`; explicit values are case-insensitive and normalized to the CMS application name. Blank or unknown values return no settings. |
| `language` | `string?` | `null`  | Language branch. Uses the preferred culture when `null`.             |

### 3. Edit settings in the CMS

Settings appear under the **Site Settings** navigation component in the CMS assets pane. Editors can manage settings per site and per language, just like regular content.

## How it works

1. **Initialization** — On application startup, the module registers a content root named `SettingsRoot` under the CMS root page. It reconciles every site and every `[SettingsContentType]`, creating missing folders and settings content.

2. **Caching** — Settings are cached in `ISynchronizedObjectInstanceCache` per site, content type, and language, with separate entries for published and draft modes. Draft saves invalidate only the local draft entry; published, language, and site lifecycle changes synchronize invalidation across nodes. Cache fills use bounded lock stripes to avoid duplicate work without globally serializing requests. Missing settings for an existing site use a one-minute negative cache; unknown site IDs are never cached.

3. **Retrieval** — `GetSiteSettings<T>()` reads from cache, resolving language fallback chains from `IContentLanguageSettingsHandler`. In edit mode, a common draft is returned when available; otherwise the published version is used. Default mode returns the published version.

4. **Site lifecycle** — Typed CMS event subscribers create and permanently remove settings folders when sites change. On a site rename, a default folder label follows the site name while an editor-defined label is preserved; its hidden site identifier is always updated.

## Project structure

```text
TuyenPham.SiteSettings/
├── Components/          # CMS navigation component
├── DependencyInjection/ # AddSiteSettings() extension method
├── Descriptors/         # Content repository descriptor
├── Infrastructure/      # CMS initialization module and lifecycle event subscriber
├── Models/              # SettingsBase, SettingsContentTypeAttribute, SettingsFolder
├── Providers/           # Global search provider
├── Services/            # ISettingsService and SettingsService
└── ClientResources/     # Client-side assets (styles)
```

## Running tests

The test project uses [xUnit v3](https://xunit.net/) and [NSubstitute](https://nsubstitute.github.io/) for mocking.

```shell wrap
dotnet run --project TuyenPham.SiteSettings.Tests
```

## Building the NuGet package

```shell wrap
dotnet pack --configuration Release --output ./nupkg
```

## License

See [LICENSE.txt](LICENSE.txt) for details.
