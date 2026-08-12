using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Cache;
using NSubstitute;
using TuyenPham.SiteSettings.Models;
using TuyenPham.SiteSettings.Services;

namespace TuyenPham.SiteSettings.Tests.Services;

public class SettingsServiceContentEventTests : SettingsServiceTestBase
{
    private sealed class TestSettings : SettingsBase;

    #region PublishedContent (via InitializeSettings event wiring)

    [Fact]
    public void InitializeSettings_SubscribesToContentEvents()
    {
        var service = CreateService();

        // Set up RegisterContentRoots to succeed
        SetupEmptySettingsRoot(service);

        service.InitializeSettings();

        ContentEvents.Received().PublishedContent += Arg.Any<EventHandler<ContentEventArgs>>();
        ContentEvents.Received().SavedContent += Arg.Any<EventHandler<ContentEventArgs>>();
        ContentEvents.Received().MovedContent += Arg.Any<EventHandler<ContentEventArgs>>();
        ContentEvents.Received().DeletedContentLanguage += Arg.Any<EventHandler<ContentEventArgs>>();
    }

    [Fact]
    public void InitializeSettings_CalledTwice_SubscribesToEachEventOnce()
    {
        var service = CreateService();
        SetupEmptySettingsRoot(service);

        service.InitializeSettings();
        service.InitializeSettings();

        ContentEvents.Received(1).PublishedContent += Arg.Any<EventHandler<ContentEventArgs>>();
        ContentEvents.Received(1).SavedContent += Arg.Any<EventHandler<ContentEventArgs>>();
    }

    [Fact]
    public void UninitializeSettings_UnsubscribesFromContentEvents()
    {
        var service = CreateService();
        SetupEmptySettingsRoot(service);
        service.InitializeSettings();

        service.UninitializeSettings();

        ContentEvents.Received().PublishedContent -= Arg.Any<EventHandler<ContentEventArgs>>();
        ContentEvents.Received().SavedContent -= Arg.Any<EventHandler<ContentEventArgs>>();
        ContentEvents.Received().MovedContent -= Arg.Any<EventHandler<ContentEventArgs>>();
        ContentEvents.Received().DeletedContentLanguage -= Arg.Any<EventHandler<ContentEventArgs>>();
    }

    [Fact]
    public void SavedContent_InvalidatesOnlyTheLocalDraftCache()
    {
        EventHandler<ContentEventArgs>? savedHandler = null;
        ContentEvents
            .When(events => events.SavedContent += Arg.Any<EventHandler<ContentEventArgs>>())
            .Do(call => savedHandler = call.Arg<EventHandler<ContentEventArgs>>());
        var service = CreateService();
        SetupEmptySettingsRoot(service);
        var folderLink = CreateContentReference(10);
        var folder = Substitute.For<SettingsFolder>();
        folder.SiteId.Returns("MySite");
        var settings = Substitute.For<SettingsBase>();
        settings.ParentLink.Returns(folderLink);

        ContentRepository
            .TryGet(folderLink, out Arg.Any<SettingsFolder>())
            .Returns(call =>
            {
                call[1] = folder;
                return true;
            });
        ApplicationRepository.Get("MySite").Returns(CreateWebsite("MySite"));
        service.InitializeSettings();

        savedHandler!(this, new ContentEventArgs(settings));

        CacheManager.Received(1).RemoveLocal(Arg.Any<string>());
        CacheManager.DidNotReceive().Remove(Arg.Any<string>());
    }

    [Fact]
    public void SavedContent_WhenParentCannotBeResolved_ClearsSynchronizedCaches()
    {
        EventHandler<ContentEventArgs>? savedHandler = null;
        ContentEvents
            .When(events => events.SavedContent += Arg.Any<EventHandler<ContentEventArgs>>())
            .Do(call => savedHandler = call.Arg<EventHandler<ContentEventArgs>>());
        var service = CreateService();
        SetupEmptySettingsRoot(service);
        var settings = Substitute.For<SettingsBase>();
        settings.ParentLink.Returns(CreateContentReference(10));
        service.InitializeSettings();

        savedHandler!(this, new ContentEventArgs(settings));

        CacheManager.Received().Remove("TuyenPham-SiteSettings");
        CacheManager.Received().Remove("TuyenPham-SiteSettings-LanguageSettings");
    }

    [Fact]
    public void GetSiteSettings_ForUnknownSite_DoesNotCreateNegativeCacheEntries()
    {
        var service = CreateService();

        var settings = service.GetSiteSettings<TestSettings>("unknown-site", "en");

        Assert.Null(settings);
        CacheManager.DidNotReceive().Insert(
            Arg.Any<string>(),
            Arg.Any<object>(),
            Arg.Any<CacheEvictionPolicy>());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void GetSiteSettings_ForBlankSiteId_DoesNotQueryOrCache(string siteId)
    {
        var service = CreateService();

        var settings = service.GetSiteSettings<TestSettings>(siteId, "en");

        Assert.Null(settings);
        ApplicationRepository.DidNotReceive().Get(Arg.Any<string>());
        CacheManager.DidNotReceive().Get(Arg.Any<string>());
        CacheManager.DidNotReceive().Insert(
            Arg.Any<string>(),
            Arg.Any<object>(),
            Arg.Any<CacheEvictionPolicy>());
    }

    [Fact]
    public void GetSiteSettings_WhenRootIsMissing_CreatesShortNegativeCacheEntries()
    {
        var service = CreateService();
        ApplicationRepository.Get("MySite").Returns(CreateWebsite("MySite"));
        ContentRootService.List().Returns([]);
        ContentRepository
            .GetItems(Arg.Any<IEnumerable<ContentReference>>(), Arg.Any<LoaderOptions>())
            .Returns([]);

        var settings = service.GetSiteSettings<TestSettings>("MySite", "en");

        Assert.Null(settings);
        ApplicationRepository.Received(1).Get("MySite");
        CacheManager.Received(2).Insert(
            Arg.Any<string>(),
            Arg.Any<object>(),
            Arg.Any<CacheEvictionPolicy>());
        var cachePolicies = CacheManager.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ISynchronizedObjectInstanceCache.Insert))
            .Select(call => Assert.IsType<CacheEvictionPolicy>(call.GetArguments()[2]));

        Assert.All(cachePolicies, policy => Assert.Equal(TimeSpan.FromMinutes(1), policy.Expiration));
    }

    [Fact]
    public void GetSiteSettings_WithDifferentSiteIdCasing_UsesCanonicalCacheKeys()
    {
        var service = CreateService();
        ApplicationRepository.Get(Arg.Any<string>()).Returns(CreateWebsite("MySite"));
        ContentRootService.List().Returns([]);
        ContentRepository
            .GetItems(Arg.Any<IEnumerable<ContentReference>>(), Arg.Any<LoaderOptions>())
            .Returns([]);

        var settings = service.GetSiteSettings<TestSettings>("MYSITE", "en");

        Assert.Null(settings);
        ApplicationRepository.Received(1).Get("MYSITE");
        var insertedKeys = CacheManager.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ISynchronizedObjectInstanceCache.Insert))
            .Select(call => Assert.IsType<string>(call.GetArguments()[0]));

        Assert.Equal(2, insertedKeys.Count());
        Assert.All(insertedKeys, key =>
            Assert.StartsWith("TuyenPham-SiteSettings-MySite-", key, StringComparison.Ordinal));
    }

    #endregion

    #region InitializeSettings

    [Fact]
    public void InitializeSettings_RegistersContentRootsWhenNotRegistered()
    {
        var service = CreateService();
        SetupEmptySettingsRoot(service);

        service.InitializeSettings();

        ContentRootService.Received().Register<SettingsFolder>(
            SettingsFolder.SettingsRootName,
            SettingsFolder.SettingsRootGuid,
            ContentReference.RootPage);
    }

    [Fact]
    public void InitializeSettings_SkipsRegistrationWhenRootAlreadyRegistered()
    {
        var service = CreateService();
        var rootRef = CreateContentReference(10);
        var rootContent = Substitute.For<IContent>();
        rootContent.ContentGuid.Returns(SettingsFolder.SettingsRootGuid);
        rootContent.Name.Returns(SettingsFolder.SettingsRootName);
        rootContent.ContentLink.Returns(rootRef);

        ContentRootService.List().Returns([rootRef]);
        ContentRepository
            .GetItems(Arg.Any<IEnumerable<ContentReference>>(), Arg.Any<LoaderOptions>())
            .Returns([rootContent]);
        ApplicationRepository.List().Returns([]);
        ContentRepository
            .GetChildren<SettingsFolder>(rootRef)
            .Returns([]);

        service.InitializeSettings();

        ContentRootService.DidNotReceive().Register<SettingsFolder>(
            Arg.Any<string>(),
            Arg.Any<Guid>(),
            Arg.Any<ContentReference>());
    }

    [Fact]
    public void InitializeSettings_WhenRegisterContentRootsThrowsNotSupportedException_Throws()
    {
        var service = CreateService();
        ContentRootService.List().Returns([]);
        ContentRepository
            .GetItems(Arg.Any<IEnumerable<ContentReference>>(), Arg.Any<LoaderOptions>())
            .Returns([]);
        ContentRootService
            .When(x => x.Register<SettingsFolder>(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<ContentReference>()))
            .Do(_ => throw new NotSupportedException("Root already registered with different GUID"));

        Assert.Throws<NotSupportedException>(() => service.InitializeSettings());
    }

    #endregion

    private void SetupEmptySettingsRoot(SettingsService service)
    {
        ContentRootService.List().Returns([]);
        ContentRepository
            .GetItems(Arg.Any<IEnumerable<ContentReference>>(), Arg.Any<LoaderOptions>())
            .Returns([]);
        ApplicationRepository.List().Returns([]);
    }
}
