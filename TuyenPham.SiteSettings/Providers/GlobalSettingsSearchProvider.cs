using System.Collections.Generic;
using EPiServer;
using EPiServer.Applications;
using EPiServer.Cms.Shell.Search;
using EPiServer.Shell;
using EPiServer.Shell.Search;
using EPiServer.Web.Routing;
using TuyenPham.SiteSettings.Models;
using TuyenPham.SiteSettings.Services;

namespace TuyenPham.SiteSettings.Providers;

/// <summary>
/// Search provider that enables finding site settings content in the Optimizely CMS global search.
/// Searches settings by name within the global settings root.
/// </summary>
[SearchProvider]
// ReSharper disable once ClassNeverInstantiated.Global
public class GlobalSettingsSearchProvider(
    LocalizationService localizationService,
    IApplicationResolver siteDefinitionResolver,
    IContentTypeRepository contentTypeRepository,
    EditUrlResolver editUrlResolver,
    IContentLanguageAccessor contentLanguageAccessor,
    IUrlResolver urlResolver,
    UIDescriptorRegistry uiDescriptorRegistry,
    IContentLoader contentLoader,
    ISettingsService settingsService)
    : ContentSearchProviderBase<SettingsBase, ContentType>(
        localizationService,
        siteDefinitionResolver,
        contentTypeRepository,
        editUrlResolver,
        contentLanguageAccessor,
        urlResolver,
        uiDescriptorRegistry)
{
    /// <summary>
    /// The search area identifier used to scope search results to global settings.
    /// </summary>
    internal const string SearchArea = "Settings/globalsettings";

    /// <inheritdoc />
    public override string Area => SearchArea;

    /// <inheritdoc />
    public override string Category => LocalizationService.GetString("/episerver/cms/components/globalSettings/title", "Site Settings");

    /// <inheritdoc />
    protected override string IconCssClass => "epi-iconSettings";

    /// <summary>
    /// Searches the global settings for content matching the specified query string by name.
    /// Requires a minimum of 2 characters in the search query.
    /// </summary>
    /// <param name="query">The search query containing the search text and max results.</param>
    /// <returns>A collection of <see cref="SearchResult"/> matching the query.</returns>
    public override IEnumerable<SearchResult> Search(Query query)
    {
        if (string.IsNullOrWhiteSpace(query.SearchQuery)
            || query.SearchQuery.Trim().Length < 2)
        {
            return [];
        }

        var searchResultList = new List<SearchResult>();
        var str = query.SearchQuery.Trim();

        var globalSettings = contentLoader
            .GetChildren<SettingsBase>(settingsService.GlobalSettingsRoot);

        foreach (var setting in globalSettings)
        {
            if (setting.Name.IndexOf(str, StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            searchResultList.Add(CreateSearchResult(setting));

            if (searchResultList.Count == query.MaxResults)
            {
                break;
            }
        }

        return searchResultList;
    }

    /// <summary>
    /// Creates preview text for a settings content item in search results.
    /// </summary>
    /// <param name="content">The content data to generate preview text for.</param>
    /// <returns>A preview string combining the settings name and localized label, or empty if content is not <c>null</c>.</returns>
    protected override string CreatePreviewText(IContentData? content)
    {
        return content == null
            ? $"{(content as SettingsBase)?.Name} {LocalizationService.GetString("/contentRepositories/globalsettings/customSelectTitle", "Settings").ToLower()}"
            : string.Empty;
    }

    /// <summary>
    /// Gets the edit URL for a settings content item, pointing to the custom settings editor view.
    /// </summary>
    /// <param name="contentData">The settings content to get the edit URL for.</param>
    /// <param name="onCurrentHost">Always set to <c>true</c> as settings are edited on the current host.</param>
    /// <returns>The URL to the settings editor for the specified content, or an empty string if <paramref name="contentData"/> is <c>null</c>.</returns>
    protected override string GetEditUrl(
        SettingsBase? contentData,
        out bool onCurrentHost)
    {
        onCurrentHost = true;

        if (contentData == null)
        {
            return string.Empty;
        }

        var contentLink = contentData.ContentLink;

        var language = contentData is ILocalizable localizable
            ? localizable.Language.Name
            : string.Empty;

        // ReSharper disable StringLiteralTypo
        return $"/episerver/TuyenPham.Cms.Settings/settings#context=epi.cms.contentdata:///{contentLink.ID}&viewsetting=viewlanguage:///{language}";
        // ReSharper restore StringLiteralTypo
    }
}