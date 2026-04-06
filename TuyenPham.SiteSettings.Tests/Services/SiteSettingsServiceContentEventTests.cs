using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using NSubstitute;
using TuyenPham.SiteSettings.Models;
using TuyenPham.SiteSettings.Services;

namespace TuyenPham.SiteSettings.Tests.Services;

public class SiteSettingsServiceContentEventTests : SiteSettingsServiceTestBase
{
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

    private void SetupEmptySettingsRoot(SiteSettingsService service)
    {
        ContentRootService.List().Returns([]);
        ContentRepository
            .GetItems(Arg.Any<IEnumerable<ContentReference>>(), Arg.Any<LoaderOptions>())
            .Returns([]);
        ApplicationRepository.List().Returns([]);
    }
}
