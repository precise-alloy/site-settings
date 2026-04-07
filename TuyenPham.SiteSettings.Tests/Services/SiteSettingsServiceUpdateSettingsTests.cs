using EPiServer.Core;
using EPiServer.DataAbstraction;
using NSubstitute;
using TuyenPham.SiteSettings.Models;

namespace TuyenPham.SiteSettings.Tests.Services;

public class SettingsServiceUpdateSettingsTests : SettingsServiceTestBase
{
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

        service.UpdateSettings();

        ContentRepository.Received().GetDefault<SettingsFolder>(Arg.Any<ContentReference>());
    }
}
