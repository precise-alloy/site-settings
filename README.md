# TuyenPham.SiteSettings

A plugin for [Optimizely CMS](https://www.optimizely.com/products/content-management/) that adds per-site settings management directly in the CMS editor UI. Define strongly-typed settings content types and retrieve them in code with full multi-language and multi-site support.

## Features

- **Per-site settings** — each site gets its own settings folder, created automatically when a site is added.
- **Strongly-typed** — define settings as C# classes with property editors, just like regular Optimizely content.
- **Multi-language** — settings respect language branches and fallback chains configured in the CMS.
- **Edit & published mode** — draft settings are available in edit mode, published settings in default mode.
- **CMS integration** — settings appear in the assets pane navigation tree and are searchable via the global search.
- **Caching** — settings are cached per site/type/language with automatic invalidation on publish, save, move, and delete.

## Requirements

- .NET 10+
- Optimizely CMS 13+ (`EPiServer.CMS.UI.Core >= 13.0.0`)

## Installation

Install from NuGet:

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

A settings instance is automatically created for each site when the site is first added.

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
| `siteId`   | `string?` | `null`  | Site identifier. Resolved from the current HTTP context when `null`. |
| `language` | `string?` | `null`  | Language branch. Uses the preferred culture when `null`.             |

### 3. Edit settings in the CMS

Settings appear under the **Site Settings** navigation component in the CMS assets pane. Editors can manage settings per site and per language, just like regular content.

## How it works

1. **Initialization** — On application startup, the module registers a content root named `SettingsRoot` under the CMS root page. For each site returned by `IApplicationRepository`, a `SettingsFolder` is created (if missing), and default settings content is created for every type decorated with `[SettingsContentType]`.

2. **Caching** — Settings are cached in `ISynchronizedObjectInstanceCache` per site, content type, and language, with separate entries for published and draft modes. Cache is invalidated on content publish, save, move, delete, and language settings changes. Cache invalidation is synchronized across CDN nodes.

3. **Retrieval** — `GetSiteSettings<T>()` reads from cache, resolving language fallback chains from `IContentLanguageSettingsHandler`. In edit mode, draft (common draft) versions are returned; in default mode, published versions are returned.

4. **Site lifecycle** — The service listens for site created, deleted, and updated events to automatically create, remove, or rename settings folders.

## Project structure

```text
TuyenPham.SiteSettings/
├── Components/          # CMS navigation component
├── DependencyInjection/ # AddSiteSettings() extension method
├── Descriptors/         # Content repository descriptor
├── Infrastructure/      # CMS initialization module
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
