using EPiServer;
using EPiServer.Applications;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Cache;
using EPiServer.Framework.TypeScanner;
using EPiServer.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TuyenPham.SiteSettings.Services;

namespace TuyenPham.SiteSettings.Tests.Services;

public class SettingsServiceTestBase
{
    protected readonly IContentEvents ContentEvents = Substitute.For<IContentEvents>();
    protected readonly IContentLanguageSettingsHandler ContentLanguageSettingsHandler = Substitute.For<IContentLanguageSettingsHandler>();
    protected readonly IContentRepository ContentRepository = Substitute.For<IContentRepository>();
    protected readonly IContentTypeRepository ContentTypeRepository = Substitute.For<IContentTypeRepository>();
    protected readonly IContentVersionRepository ContentVersionRepository = Substitute.For<IContentVersionRepository>();
    protected readonly IContextModeResolver ContextModeResolver = Substitute.For<IContextModeResolver>();
    protected readonly IHttpContextAccessor HttpContextAccessor = Substitute.For<IHttpContextAccessor>();
    protected readonly ILogger<SettingsService> Logger = Substitute.For<ILogger<SettingsService>>();
    protected readonly IApplicationRepository ApplicationRepository = Substitute.For<IApplicationRepository>();
    protected readonly ISynchronizedObjectInstanceCache CacheManager = Substitute.For<ISynchronizedObjectInstanceCache>();
    protected readonly ITypeScannerLookup TypeScannerLookup = Substitute.For<ITypeScannerLookup>();
    protected readonly ContentRootService ContentRootService = Substitute.For<ContentRootService>();

    protected SettingsService CreateService() =>
        new(
            ContentEvents,
            ContentLanguageSettingsHandler,
            ContentRepository,
            ContentTypeRepository,
            ContentVersionRepository,
            ContextModeResolver,
            HttpContextAccessor,
            Logger,
            ApplicationRepository,
            CacheManager,
            TypeScannerLookup,
            ContentRootService);

    protected static Website CreateWebsite(string name = "TestSite")
    {
        return new Website(name, new ContentReference(1));
    }

    protected static ContentReference CreateContentReference(int id = 100) => new(id);
}
