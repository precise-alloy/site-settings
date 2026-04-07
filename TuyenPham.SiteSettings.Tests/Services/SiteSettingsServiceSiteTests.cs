using EPiServer.Applications;
using EPiServer.Core;
using NSubstitute;
using TuyenPham.SiteSettings.Models;

namespace TuyenPham.SiteSettings.Tests.Services;

public class SettingsServiceSiteTests : SettingsServiceTestBase
{
    #region SiteCreated

    [Fact]
    public void SiteCreated_WhenSenderIsNull_DoesNothing()
    {
        var service = CreateService();
        var site = CreateWebsite("TestSite");
        var e = new ApplicationCreatedEvent(site);

        service.SiteCreated(null, e);

        ContentRepository.DidNotReceive().GetChildren<SettingsFolder>(Arg.Any<ContentReference>());
    }

    [Fact]
    public void SiteCreated_WhenFolderDoesNotExist_CreatesSiteFolder()
    {
        var service = CreateService();
        var rootRef = CreateContentReference(10);
        service.GlobalSettingsRoot = rootRef;

        var site = CreateWebsite("NewSite");
        var e = new ApplicationCreatedEvent(site);

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

        service.SiteCreated(this, e);

        ContentRepository.Received().GetDefault<SettingsFolder>(Arg.Any<ContentReference>());
    }

    [Fact]
    public void SiteCreated_WhenFolderAlreadyExists_DoesNotCreateFolder()
    {
        var service = CreateService();
        var rootRef = CreateContentReference(10);
        service.GlobalSettingsRoot = rootRef;

        var site = CreateWebsite("ExistingSite");
        var e = new ApplicationCreatedEvent(site);

        var existingFolder = Substitute.For<SettingsFolder>();
        existingFolder.Name.Returns("ExistingSite");

        ContentRepository
            .GetChildren<SettingsFolder>(rootRef)
            .Returns([existingFolder]);

        service.SiteCreated(this, e);

        ContentRepository.DidNotReceive().GetDefault<SettingsFolder>(Arg.Any<ContentReference>());
    }

    #endregion

    #region SiteDeleted

    [Fact]
    public void SiteDeleted_WhenSenderIsNull_DoesNothing()
    {
        var service = CreateService();
        var site = CreateWebsite("TestSite");
        var e = new ApplicationDeletedEvent(site);

        service.SiteDeleted(null, e);

        ContentRepository.DidNotReceive().GetChildren<SettingsFolder>(Arg.Any<ContentReference>());
    }

    [Fact]
    public void SiteDeleted_WhenFolderExists_DeletesFolderAndClearsCache()
    {
        var service = CreateService();
        var rootRef = CreateContentReference(10);
        service.GlobalSettingsRoot = rootRef;

        var site = CreateWebsite("MySite");
        var e = new ApplicationDeletedEvent(site);

        var folder = Substitute.For<SettingsFolder>();
        folder.Name.Returns("MySite");
        var folderRef = CreateContentReference(20);
        folder.ContentLink.Returns(folderRef);

        ContentRepository
            .GetChildren<SettingsFolder>(rootRef)
            .Returns([folder]);

        service.SiteDeleted(this, e);

        ContentRepository.Received().Delete(folderRef, true, EPiServer.Security.AccessLevel.NoAccess);
        CacheManager.Received().Remove("TuyenPham-SiteSettings");
    }

    [Fact]
    public void SiteDeleted_WhenFolderDoesNotExist_DoesNotDelete()
    {
        var service = CreateService();
        var rootRef = CreateContentReference(10);
        service.GlobalSettingsRoot = rootRef;

        var site = CreateWebsite("NonExistentSite");
        var e = new ApplicationDeletedEvent(site);

        ContentRepository
            .GetChildren<SettingsFolder>(rootRef)
            .Returns([]);

        service.SiteDeleted(this, e);

        ContentRepository.DidNotReceive().Delete(Arg.Any<ContentReference>(), Arg.Any<bool>(), Arg.Any<EPiServer.Security.AccessLevel>());
    }

    #endregion

    #region SiteUpdated

    [Fact]
    public void SiteUpdated_WhenSenderIsNull_DoesNothing()
    {
        var service = CreateService();
        var site = CreateWebsite("TestSite");
        var e = new ApplicationUpdatedEvent(site, site);

        service.SiteUpdated(null, e);

        ContentRepository.DidNotReceive().GetChildren<IContent>(Arg.Any<ContentReference>());
    }

    [Fact]
    public void SiteUpdated_WhenFolderExists_RenamesFolder()
    {
        var service = CreateService();
        var rootRef = CreateContentReference(10);
        service.GlobalSettingsRoot = rootRef;

        var prevSite = CreateWebsite("OldName");
        var updatedSite = CreateWebsite("NewName");
        var e = new ApplicationUpdatedEvent(updatedSite, prevSite);

        var existingFolder = new ContentFolder();
        existingFolder.Name = "OldName";

        ContentRepository
            .GetChildren<IContent>(rootRef)
            .Returns(new List<IContent> { existingFolder });

        service.SiteUpdated(this, e);

        ContentRepository.Received().Save(
            Arg.Any<IContent>(),
            EPiServer.DataAccess.SaveAction.Publish,
            EPiServer.Security.AccessLevel.NoAccess);
    }

    [Fact]
    public void SiteUpdated_WhenFolderDoesNotExist_CreatesSiteFolder()
    {
        var service = CreateService();
        var rootRef = CreateContentReference(10);
        service.GlobalSettingsRoot = rootRef;

        var prevSite = CreateWebsite("OldName");
        var updatedSite = CreateWebsite("NewName");
        var e = new ApplicationUpdatedEvent(updatedSite, prevSite);

        ContentRepository
            .GetChildren<IContent>(rootRef)
            .Returns(new List<IContent>());
        TypeScannerLookup.AllTypes.Returns([]);

        var newFolder = new SettingsFolder();
        ContentRepository
            .GetDefault<SettingsFolder>(Arg.Any<ContentReference>())
            .Returns(newFolder);
        ContentRepository
            .Save(Arg.Any<IContent>(), Arg.Any<EPiServer.DataAccess.SaveAction>(), Arg.Any<EPiServer.Security.AccessLevel>())
            .Returns(CreateContentReference(20));

        service.SiteUpdated(this, e);

        ContentRepository.Received().GetDefault<SettingsFolder>(Arg.Any<ContentReference>());
    }

    #endregion
}
