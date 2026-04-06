using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Applications;
using EPiServer.Cms.Shell;
using EPiServer.DataAccess;
using EPiServer.Framework.Cache;
using EPiServer.Framework.TypeScanner;
using EPiServer.Globalization;
using EPiServer.Security;
using EPiServer.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TuyenPham.SiteSettings.Models;

namespace TuyenPham.SiteSettings.Services;

/// <summary>
/// Manages site settings content, including caching, initialization, and content event handling.
/// Settings are cached per site, per content type, and per language, with separate caches for edit and published modes.
/// </summary>
public partial class SiteSettingsService(
    IContentEvents contentEvents,
    IContentLanguageSettingsHandler contentLanguageSettingsHandler,
    IContentRepository contentRepository,
    IContentTypeRepository contentTypeRepository,
    IContentVersionRepository contentVersionRepository,
    IContextModeResolver contextModeResolver,
    IHttpContextAccessor httpContextAccessor,
    ILogger<SiteSettingsService> logger,
    IApplicationRepository applicationRepository,
    ISynchronizedObjectInstanceCache cacheManager,
    ITypeScannerLookup typeScannerLookup,
    ContentRootService contentRootService)
        : ISiteSettingsService
{
    private const string SettingServicesMasterCacheKey = "TuyenPham-SiteSettings";
    private const string LanguageSettingsCacheKey = "TuyenPham-SiteSettings-LanguageSettings";

    private readonly IContentEvents _contentEvents = contentEvents;
    private readonly IContentLanguageSettingsHandler _contentLanguageSettingsHandler = contentLanguageSettingsHandler;
    private readonly IContentRepository _contentRepository = contentRepository;
    private readonly IContentTypeRepository _contentTypeRepository = contentTypeRepository;
    private readonly IContentVersionRepository _contentVersionRepository = contentVersionRepository;
    private readonly IContextModeResolver _contextModeResolver = contextModeResolver;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ILogger<SiteSettingsService> _logger = logger;
    private readonly IApplicationRepository _applicationRepository = applicationRepository;
    private readonly ISynchronizedObjectInstanceCache _cacheManager = cacheManager;
    private readonly ITypeScannerLookup _typeScannerLookup = typeScannerLookup;
    private readonly ContentRootService _contentRootService = contentRootService;
    /// <inheritdoc />
    public ContentReference? GlobalSettingsRoot { get; set; }

    /// <inheritdoc />
    public void InitializeSettings()
    {
        try
        {
            RegisterContentRoots();
        }
        catch (NotSupportedException notSupportedException)
        {
            _logger.LogError(notSupportedException, "[Settings] {message}", notSupportedException.Message);
            throw;
        }

        ContentLanguageSettingRepository.ContentLanguageSettingDeleted += ContentLanguageSettingSavedOrDeleted;
        ContentLanguageSettingRepository.ContentLanguageSettingSaved += ContentLanguageSettingSavedOrDeleted;
        _contentEvents.PublishedContent += PublishedContent;
        _contentEvents.SavedContent += SavedContent;
        _contentEvents.MovedContent += MovedContent;
        _contentEvents.DeletedContentLanguage += DeletedContentLanguage;
    }

    /// <summary>
    /// Refreshes the settings root reference and repopulates the cache for all sites and their settings.
    /// Creates missing site folders for sites that do not yet have one.
    /// </summary>
    public void UpdateSettings()
    {
        var root = _contentRepository.GetItems(
                _contentRootService.List(),
                new LoaderOptions())
            .FirstOrDefault(x => x.ContentGuid == SettingsFolder.SettingsRootGuid);

        if (root == null)
        {
            _logger.LogWarning("[Settings] Setting root is NULL");
            return;
        }

        GlobalSettingsRoot = root.ContentLink;
        var children = _contentRepository.GetChildren<SettingsFolder>(GlobalSettingsRoot).ToList();
        foreach (var site in _applicationRepository.List())
        {
            var folder = children.Find(x => x.Name.Equals(site.Name, StringComparison.InvariantCultureIgnoreCase));
            if (folder != null)
            {
                var settingsTypes = new List<Type>();
                foreach (var child in _contentRepository.GetChildren<SettingsBase>(folder.ContentLink,
                             [LanguageLoaderOption.MasterLanguage()]))
                {
                    var settingType = child.GetOriginalType();
                    if (settingsTypes.Contains(settingType))
                    {
                        _logger.LogWarning($"[Settings] Setting of type {settingType.Name} for site {folder.Name} have more than one instance");
                    }
                    else
                    {
                        settingsTypes.Add(settingType);
                    }
                    RepopulateCacheForAllLanguage(site.Name, child);
                }
            }
            else
            {
                CreateSiteFolder(site);
            }
        }
    }

    /// <inheritdoc />
    public T? GetSiteSettings<T>(
        string? siteId = null,
        string? language = null)
        where T : SettingsBase
    {
        var contentType = typeof(T);
        if (contentType.IsInterface)
        {
            var registerType = ServiceLocator.Current.GetInstance<T>();
            contentType = registerType.GetType();
        }
        var contentLanguage = language ?? ContentLanguage.PreferredCulture.Name;
        if (siteId is null)
        {
            siteId = ResolveSiteId();
            if (siteId is null)
            {
                return default;
            }
        }
        try
        {
            var settings = GetSettingFromCache(
                siteId, contentType,
                _contextModeResolver.CurrentMode == ContextMode.Edit);

            if (settings.FirstOrDefault() is not { Value: { } } setting)
            {
                return default;
            }

            var languageIncludeFallback = GetFallbackLanguage(contentLanguage, setting.Value.ContentLink);
            languageIncludeFallback.Insert(0, contentLanguage);
            var matchedLanguage = languageIncludeFallback.FirstOrDefault(x => settings.ContainsKey(x)); //Get requesting language or fallback
            if (!string.IsNullOrEmpty(matchedLanguage))
            {
                return settings[matchedLanguage] as T;
            }

            return settings.Values.FirstOrDefault(x => x.IsMasterLanguageBranch()) as T;

        }
        catch (ArgumentNullException argumentNullException)
        {
            _logger.LogError(argumentNullException, $"[Settings] {argumentNullException.Message}");
        }

        return default;
    }

    /// <summary>
    /// Registers the settings root in the content root service if not already registered, then calls <see cref="UpdateSettings"/>.
    /// </summary>
    private void RegisterContentRoots()
    {
        var registeredRoots = _contentRepository.GetItems(_contentRootService.List(), new LoaderOptions());
        var settingsRootRegistered = registeredRoots
            .Any(x => x.ContentGuid == SettingsFolder.SettingsRootGuid
                      && x.Name.Equals(SettingsFolder.SettingsRootName));

        if (!settingsRootRegistered)
        {
            _contentRootService.Register<SettingsFolder>(
                SettingsFolder.SettingsRootName,
                SettingsFolder.SettingsRootGuid,
                ContentReference.RootPage);
        }

        UpdateSettings();
    }

    /// <summary>
    /// Retrieves cached settings for the specified site and type. If the cache is empty, repopulates it.
    /// </summary>
    /// <param name="siteId">The site identifier.</param>
    /// <param name="type">The settings content type.</param>
    /// <param name="isEditMod">Whether to retrieve the draft (edit mode) version.</param>
    /// <returns>A dictionary of language branch to settings instance.</returns>
    private Dictionary<string, SettingsBase?> GetSettingFromCache(string siteId, Type type, bool isEditMod)
    {
        //If cache cleared
        if (_cacheManager.Get(CreateCacheKey(siteId, type, isEditMod)) is Dictionary<string, SettingsBase?>
            settingsOfType)
        {
            return settingsOfType;
        }

        if (!RepopulateCacheForAllLanguage(siteId, type))
        {
            InsertSettingToCache(siteId, type, false, new Dictionary<string, SettingsBase?>());
            InsertSettingToCache(siteId, type, true, new Dictionary<string, SettingsBase?>());
            _logger.LogWarning($"[Settings] no setting available for type {type} in site {siteId}");
        }

        return _cacheManager.Get(CreateCacheKey(siteId, type, isEditMod)) as Dictionary<string, SettingsBase?>
               ?? new Dictionary<string, SettingsBase?>();

    }

    /// <summary>
    /// Repopulates the cache for all language branches of a given settings type for the specified site.
    /// </summary>
    /// <param name="siteId">The site identifier.</param>
    /// <param name="type">The settings content type to repopulate.</param>
    /// <returns><c>true</c> if settings were found and cached; otherwise, <c>false</c>.</returns>
    private bool RepopulateCacheForAllLanguage(string siteId, Type type)
    {
        var root = _contentRepository.GetItems(_contentRootService.List(), new LoaderOptions())
            .FirstOrDefault(x => x.ContentGuid == SettingsFolder.SettingsRootGuid)
                   ?? _contentRepository.Get<IContent>(SettingsFolder.SettingsRootGuid);
        if (root == null)
        {
            _logger.LogWarning("[Settings] Setting root is NULL");
            return false;
        }

        var site = _applicationRepository.Get(siteId);
        if (site == null)
        {
            return false;
        }

        var folder = _contentRepository
            .GetChildren<SettingsFolder>(root.ContentLink)
            .FirstOrDefault(x => x.Name.Equals(site.Name, StringComparison.InvariantCultureIgnoreCase));

        if (folder == null)
        {
            return false;
        }

        var settings = _contentRepository.GetChildren<SettingsBase>(
                folder.ContentLink,
                [LanguageLoaderOption.MasterLanguage()])
            .FirstOrDefault(x => type == x.GetOriginalType());

        if (settings == null)
        {
            return false;
        }
        else
        {
            RepopulateCacheForAllLanguage(siteId, settings);
        }
        return true;
    }

    /// <summary>
    /// Repopulates both published and draft caches for every existing language of the given settings content.
    /// </summary>
    /// <param name="siteId">The site identifier.</param>
    /// <param name="settings">The settings content instance (master language branch) to repopulate.</param>
    // ReSharper disable once SuggestBaseTypeForParameter
    private void RepopulateCacheForAllLanguage(string siteId, SettingsBase settings)
    {
        var publishedSettings = new Dictionary<string, SettingsBase?>();
        var draftSettings = new Dictionary<string, SettingsBase?>();
        foreach (var lang in settings.ExistingLanguages)
        {
            var setting = _contentRepository
                .Get<SettingsBase>(
                    settings.ContentLink.ToReferenceWithoutVersion(),
                    [LanguageLoaderOption.FallbackWithMaster(lang)]);
            publishedSettings[lang.Name] = setting;
            draftSettings[lang.Name] = setting;

            // add draft (not published version) settings
            var draftContentLink = _contentVersionRepository.LoadCommonDraft(settings.ContentLink, lang.Name);
            if (draftContentLink != null)
            {
                var settingsDraft = _contentRepository.Get<SettingsBase>(draftContentLink.ContentLink);
                draftSettings[lang.Name] = settingsDraft;
            }
        }

        InsertSettingToCache(siteId, settings.GetOriginalType(), false, publishedSettings);
        InsertSettingToCache(siteId, settings.GetOriginalType(), true, draftSettings);
    }

    /// <summary>
    /// Inserts a settings dictionary into the synchronized cache with a 30-day absolute expiration.
    /// </summary>
    /// <param name="siteId">The site identifier.</param>
    /// <param name="type">The settings content type.</param>
    /// <param name="isEditMode">Whether this is the edit mode (draft) cache entry.</param>
    /// <param name="settingOfType">The dictionary of language branch to settings instance.</param>
    private void InsertSettingToCache(
        string siteId,
        Type type,
        bool isEditMode,
        Dictionary<string, SettingsBase?> settingOfType)
    {
        _cacheManager.Insert(
            CreateCacheKey(siteId, type, isEditMode),
            settingOfType,
            new CacheEvictionPolicy(
                TimeSpan.FromDays(30),
                CacheTimeoutType.Absolute,
                Enumerable.Empty<string>(),
                new[] { SettingServicesMasterCacheKey }));
    }

    /// <summary>
    /// Removes both published and draft cache entries for the specified settings type and site.
    /// </summary>
    /// <typeparam name="T">The settings type.</typeparam>
    /// <param name="siteId">The site identifier.</param>
    /// <param name="settings">The settings instance whose type determines the cache keys to remove.</param>
    /// <remarks>Uses <see cref="ISynchronizedObjectInstanceCache"/> to remove cache from all CDN servers.</remarks>
    private void RemoveCache<T>(
        string siteId,
        T? settings)
        where T : SettingsBase
    {
        if (settings == null)
        {
            return;
        }
        var type = settings.GetOriginalType();
        _cacheManager.Remove(CreateCacheKey(siteId, type, true));
        _cacheManager.Remove(CreateCacheKey(siteId, type, false));
    }

    /// <summary>
    /// Clears all settings caches by removing the master cache key and the language settings cache key.
    /// </summary>
    public void ClearCache()
    {
        _cacheManager.Remove(SettingServicesMasterCacheKey);
        _cacheManager.Remove(LanguageSettingsCacheKey);
    }

    /// <summary>
    /// Gets the configured fallback language chain for the specified language and settings content reference.
    /// Results are cached with a 30-day absolute expiration.
    /// </summary>
    /// <param name="language">The language branch to look up fallbacks for.</param>
    /// <param name="settingsRef">The content reference of the settings item.</param>
    /// <returns>A list of fallback language branch names, or an empty list if none are configured.</returns>
    private List<string> GetFallbackLanguage(string language, ContentReference settingsRef)
    {
        var cacheKey = CreateLanguageSettingsCacheKey(settingsRef);
        if (_cacheManager.Get(cacheKey) is not Dictionary<string, List<string>> languageSettings)
        {
            languageSettings = _contentLanguageSettingsHandler
                .Get(settingsRef)
                .Where(x => x.IsActive)
                .ToDictionary(
                    x => x.LanguageBranch,
                    x => x.LanguageBranchFallback.ToList());

            _cacheManager.Insert(
                cacheKey,
                languageSettings,
                new CacheEvictionPolicy(
                    TimeSpan.FromDays(30),
                    CacheTimeoutType.Absolute,
                    Enumerable.Empty<string>(),
                    new[]
                    {
                        LanguageSettingsCacheKey
                    }));
        }

        return languageSettings.TryGetValue(language, out var fallBacks)
            ? fallBacks.ToList() //Clone List<>
            : [];
    }

    /// <summary>
    /// Creates a new settings folder for the given site and populates it with default instances of all registered settings types.
    /// </summary>
    /// <param name="siteDefinition">The application (site) to create a settings folder for.</param>
    private void CreateSiteFolder(Application siteDefinition)
    {
        var site = siteDefinition as Website;
        var folder = _contentRepository.GetDefault<SettingsFolder>(GlobalSettingsRoot);
        folder.Name = siteDefinition.Name;
        var reference = _contentRepository.Save(folder, SaveAction.Publish, AccessLevel.NoAccess);

        var settingsModelTypes = _typeScannerLookup
            .AllTypes
            .Where(t => t.GetCustomAttributes(typeof(SettingsContentTypeAttribute), false).Length > 0);

        foreach (var settingsType in settingsModelTypes)
        {
            if (!(settingsType.GetCustomAttributes(typeof(SettingsContentTypeAttribute), false)
                    .FirstOrDefault() is SettingsContentTypeAttribute attribute))
            {
                continue;
            }

            var contentType = _contentTypeRepository.Load(settingsType);

            var newSettings = _contentRepository.GetDefault<IContent>(reference, contentType.ID);
            newSettings.Name = attribute.DisplayName;
            _contentRepository.Save(newSettings, SaveAction.Publish, AccessLevel.NoAccess);

            InsertSettingToCache(
                siteDefinition.Name,
                newSettings.GetOriginalType(),
                false,
                new Dictionary<string, SettingsBase?>
                {
                    [newSettings.LanguageBranch()] = newSettings as SettingsBase
                });

            InsertSettingToCache(
                siteDefinition.Name,
                newSettings.GetOriginalType(),
                true,
                new Dictionary<string, SettingsBase?>
                {
                    [newSettings.LanguageBranch()] = newSettings as SettingsBase
                });
        }
    }

    /// <summary>
    /// Creates a cache key for a specific site, settings type, and edit mode combination.
    /// </summary>
    /// <param name="siteId">The site identifier.</param>
    /// <param name="type">The settings content type.</param>
    /// <param name="isEditMode">Whether the key is for the edit mode (draft) cache.</param>
    /// <returns>The computed cache key string.</returns>
    private static string CreateCacheKey(string siteId, Type type, bool isEditMode)
    {
        return isEditMode
            ? $"{SettingServicesMasterCacheKey}-{siteId}-common-draft-{type.FullName}"
            : $"{SettingServicesMasterCacheKey}-{siteId}-{type.FullName}";
    }

    /// <summary>
    /// Creates a cache key for the language settings of a specific content reference.
    /// </summary>
    /// <param name="settingsRef">The content reference of the settings item.</param>
    /// <returns>The computed language settings cache key string.</returns>
    private static string CreateLanguageSettingsCacheKey(ContentReference settingsRef) => $"{LanguageSettingsCacheKey}-LanguageSettingsOf-{settingsRef.ID}";
}