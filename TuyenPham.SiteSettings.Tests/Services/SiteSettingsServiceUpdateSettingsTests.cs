using EPiServer.Core;
using EPiServer.DataAbstraction;
using NSubstitute;
using TuyenPham.SiteSettings.Models;

namespace TuyenPham.SiteSettings.Tests.Services;

public class SettingsServiceUpdateSettingsTests : SettingsServiceTestBase
{
    [SettingsContentType]
    private sealed class AddedSettings : SettingsBase;

    [Fact]
    public void UpdateSettings_WhenRootIsNull_LogsWarningAndReturns()
    {
        var service = CreateService();
        ContentRootService.List().Returns([]);
        ContentRepository
            .GetItems(Arg.Any<IEnumerable<ContentReference>>(), Arg.Any<LoaderOptions>())
            .Returns([]);

        service.UpdateSettings();

        Assert.Null(service.GlobalSettingsRoot);
    }

    [Fact]
    public void UpdateSettings_WhenRootExists_SetsGlobalSettingsRoot()
    {
        var service = CreateService();
        var rootRef = CreateContentReference(10);
        var rootContent = Substitute.For<IContent>();
        rootContent.ContentGuid.Returns(SettingsFolder.SettingsRootGuid);
        rootContent.ContentLink.Returns(rootRef);

        ContentRootService.List().Returns([rootRef]);
        ContentRepository
            .GetItems(Arg.Any<IEnumerable<ContentReference>>(), Arg.Any<LoaderOptions>())
            .Returns([rootContent]);
        ApplicationRepository.List().Returns([]);
        ContentRepository
            .GetChildren<SettingsFolder>(rootRef)
            .Returns([]);

        service.UpdateSettings();

        Assert.Equal(rootRef, service.GlobalSettingsRoot);
    }

    [Fact]
    public void UpdateSettings_WhenSiteFolderMissing_CreatesFolder()
    {
        var service = CreateService();
        var rootRef = CreateContentReference(10);
        var rootContent = Substitute.For<IContent>();
        rootContent.ContentGuid.Returns(SettingsFolder.SettingsRootGuid);
        rootContent.ContentLink.Returns(rootRef);

        var site = CreateWebsite("MySite");

        ContentRootService.List().Returns([rootRef]);
        ContentRepository
            .GetItems(Arg.Any<IEnumerable<ContentReference>>(), Arg.Any<LoaderOptions>())
            .Returns([rootContent]);
        ApplicationRepository.List().Returns([site]);
        ContentRepository
            .GetChildren<SettingsFolder>(rootRef)
            .Returns([]);
        TypeScannerLookup.AllTypes.Returns([]);

        var newFolder = new SettingsFolder();
        ContentRepository
            .GetDefault<SettingsFolder>(Arg.Any<ContentReference>())
            .Returns(newFolder);
        ContentRepository
            .Save(Arg.Any<IContent>(), Arg.Any<EPiServer.DataAccess.SaveAction>(), Arg.Any<EPiServer.Security.AccessLevel>())
            .Returns(CreateContentReference(20));
        ContentRepository
            .Get<SettingsFolder>(Arg.Any<ContentReference>())
            .Returns(new SettingsFolder());

        service.UpdateSettings();

        ContentRepository.Received().GetDefault<SettingsFolder>(Arg.Any<ContentReference>());
    }

    [Fact]
    public void UpdateSettings_WhenSettingsTypeIsAdded_ProvisionsExistingSiteFolder()
    {
        var service = CreateService();
        var rootRef = CreateContentReference(10);
        var folderRef = CreateContentReference(20);
        var root = Substitute.For<IContent>();
        root.ContentGuid.Returns(SettingsFolder.SettingsRootGuid);
        root.ContentLink.Returns(rootRef);
        var folder = Substitute.For<SettingsFolder>();
        folder.Name.Returns("MySite");
        folder.SiteId.Returns("MySite");
        folder.ContentLink.Returns(folderRef);
        var site = CreateWebsite("MySite");
        var contentType = Substitute.For<ContentType>();
        var settings = Substitute.For<IContent>();

        ContentRootService.List().Returns([rootRef]);
        ContentRepository
            .GetItems(Arg.Any<IEnumerable<ContentReference>>(), Arg.Any<LoaderOptions>())
            .Returns([root]);
        ContentRepository.GetChildren<SettingsFolder>(rootRef).Returns([folder]);
        ContentRepository
            .GetChildren<SettingsBase>(folderRef, Arg.Any<LoaderOptions>())
            .Returns([]);
        ApplicationRepository.List().Returns([site]);
        TypeScannerLookup.AllTypes.Returns([typeof(AddedSettings)]);
        ContentTypeRepository.Load(typeof(AddedSettings)).Returns(contentType);
        ContentRepository
            .GetDefault<IContent>(folderRef, Arg.Any<int>())
            .Returns(settings);

        service.UpdateSettings();

        ContentRepository.Received().Save(settings, EPiServer.DataAccess.SaveAction.Publish, EPiServer.Security.AccessLevel.NoAccess);
    }

    [Fact]
    public void UpdateSettings_WhenFolderWasRenamed_UsesItsSiteIdInsteadOfCreatingDuplicate()
    {
        var service = CreateService();
        var rootRef = CreateContentReference(10);
        var root = Substitute.For<IContent>();
        root.ContentGuid.Returns(SettingsFolder.SettingsRootGuid);
        root.ContentLink.Returns(rootRef);
        var folder = Substitute.For<SettingsFolder>();
        folder.Name.Returns("Custom settings");
        folder.SiteId.Returns("MySite");
        folder.ContentLink.Returns(CreateContentReference(20));

        ContentRootService.List().Returns([rootRef]);
        ContentRepository.GetItems(Arg.Any<IEnumerable<ContentReference>>(), Arg.Any<LoaderOptions>()).Returns([root]);
        ContentRepository.GetChildren<SettingsFolder>(rootRef).Returns([folder]);
        ContentRepository.GetChildren<SettingsBase>(folder.ContentLink, Arg.Any<LoaderOptions>()).Returns([]);
        ApplicationRepository.List().Returns([CreateWebsite("MySite")]);
        TypeScannerLookup.AllTypes.Returns([]);

        service.UpdateSettings();

        ContentRepository.DidNotReceive().GetDefault<SettingsFolder>(Arg.Any<ContentReference>());
    }
}
