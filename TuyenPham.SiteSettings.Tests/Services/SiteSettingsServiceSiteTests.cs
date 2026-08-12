using EPiServer.Applications;
using EPiServer.Core;
using NSubstitute;
using TuyenPham.SiteSettings.Models;

namespace TuyenPham.SiteSettings.Tests.Services;

public class SettingsServiceSiteTests : SettingsServiceTestBase
{
    private sealed class TestSettingsFolder : SettingsFolder
    {
        private string _name = string.Empty;

        public override string Name
        {
            get => _name;
            set => _name = value;
        }
    }

    #region SiteCreated

    [Fact]
    public void SiteCreated_WhenSettingsAreNotInitialized_DoesNothing()
    {
        var service = CreateService();
        var site = CreateWebsite("TestSite");
        service.SiteCreated(site);

        ContentRepository.DidNotReceive().GetChildren<SettingsFolder>(Arg.Any<ContentReference>());
    }

    [Fact]
    public void SiteCreated_WhenFolderDoesNotExist_CreatesSiteFolder()
    {
        var service = CreateService();
        var rootRef = CreateContentReference(10);
        service.GlobalSettingsRoot = rootRef;

        var site = CreateWebsite("NewSite");
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

        service.SiteCreated(site);

        ContentRepository.Received().GetDefault<SettingsFolder>(Arg.Any<ContentReference>());
    }

    [Fact]
    public void SiteCreated_WhenFolderAlreadyExists_DoesNotCreateFolder()
    {
        var service = CreateService();
        var rootRef = CreateContentReference(10);
        service.GlobalSettingsRoot = rootRef;

        var site = CreateWebsite("ExistingSite");
        var existingFolder = Substitute.For<SettingsFolder>();
        existingFolder.Name.Returns("ExistingSite");

        ContentRepository
            .GetChildren<SettingsFolder>(rootRef)
            .Returns([existingFolder]);

        service.SiteCreated(site);

        ContentRepository.DidNotReceive().GetDefault<SettingsFolder>(Arg.Any<ContentReference>());
    }

    [Fact]
    public void SiteCreated_WhenRenamedFolderHasMatchingSiteId_DoesNotCreateFolder()
    {
        var service = CreateService();
        var rootRef = CreateContentReference(10);
        service.GlobalSettingsRoot = rootRef;
        var folder = Substitute.For<SettingsFolder>();
        folder.Name.Returns("Custom settings");
        folder.SiteId.Returns("ExistingSite");
        ContentRepository.GetChildren<SettingsFolder>(rootRef).Returns([folder]);

        service.SiteCreated(CreateWebsite("ExistingSite"));

        ContentRepository.DidNotReceive().GetDefault<SettingsFolder>(Arg.Any<ContentReference>());
    }

    #endregion

    #region SiteDeleted

    [Fact]
    public void SiteDeleted_WhenSettingsAreNotInitialized_DoesNothing()
    {
        var service = CreateService();
        var site = CreateWebsite("TestSite");
        service.SiteDeleted(site);

        ContentRepository.DidNotReceive().GetChildren<SettingsFolder>(Arg.Any<ContentReference>());
    }

    [Fact]
    public void SiteDeleted_WhenFolderExists_DeletesFolderAndClearsCache()
    {
        var service = CreateService();
        var rootRef = CreateContentReference(10);
        service.GlobalSettingsRoot = rootRef;

        var site = CreateWebsite("MySite");
        var folder = Substitute.For<SettingsFolder>();
        folder.Name.Returns("MySite");
        var folderRef = CreateContentReference(20);
        folder.ContentLink.Returns(folderRef);

        ContentRepository
            .GetChildren<SettingsFolder>(rootRef)
            .Returns([folder]);

        service.SiteDeleted(site);

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
        ContentRepository
            .GetChildren<SettingsFolder>(rootRef)
            .Returns([]);

        service.SiteDeleted(site);

        ContentRepository.DidNotReceive().Delete(Arg.Any<ContentReference>(), Arg.Any<bool>(), Arg.Any<EPiServer.Security.AccessLevel>());
    }

    #endregion

    #region SiteUpdated

    [Fact]
    public void SiteUpdated_WhenSettingsAreNotInitialized_DoesNothing()
    {
        var service = CreateService();
        var site = CreateWebsite("TestSite");
        service.SiteUpdated(site, site);

        ContentRepository.DidNotReceive().GetChildren<IContent>(Arg.Any<ContentReference>());
    }

    [Fact]
    public void SiteUpdated_WhenSiteNameChanges_PreservesCustomFolderNameAndUpdatesSiteId()
    {
        var service = CreateService();
        var rootRef = CreateContentReference(10);
        service.GlobalSettingsRoot = rootRef;

        var prevSite = CreateWebsite("OldName");
        var updatedSite = CreateWebsite("NewName");
        var existingFolder = new TestSettingsFolder();
        existingFolder.Name = "Custom settings";
        existingFolder.SiteId = "OldName";

        ContentRepository
            .GetChildren<SettingsFolder>(rootRef)
            .Returns([existingFolder]);

        IContent? savedFolder = null;
        ContentRepository
            .When(repository => repository.Save(
                Arg.Any<IContent>(),
                EPiServer.DataAccess.SaveAction.Publish,
                EPiServer.Security.AccessLevel.NoAccess))
            .Do(call => savedFolder = call.Arg<IContent>());

        service.SiteUpdated(updatedSite, prevSite);

        ContentRepository.Received().Save(
            Arg.Any<IContent>(),
            EPiServer.DataAccess.SaveAction.Publish,
            EPiServer.Security.AccessLevel.NoAccess);
        var savedSettingsFolder = Assert.IsType<TestSettingsFolder>(savedFolder);
        Assert.Equal("NewName", savedSettingsFolder.SiteId);
        Assert.Equal("Custom settings", savedSettingsFolder.Name);
    }

    [Fact]
    public void SiteUpdated_WhenFolderUsesDefaultLabel_UpdatesLabelAndSiteId()
    {
        var service = CreateService();
        var rootRef = CreateContentReference(10);
        service.GlobalSettingsRoot = rootRef;
        var previousSite = CreateWebsite("OldName");
        var updatedSite = CreateWebsite("NewName");
        var folder = new TestSettingsFolder { Name = "OldName", SiteId = "OldName" };
        ContentRepository.GetChildren<SettingsFolder>(rootRef).Returns([folder]);

        IContent? savedFolder = null;
        ContentRepository
            .When(repository => repository.Save(
                Arg.Any<IContent>(),
                EPiServer.DataAccess.SaveAction.Publish,
                EPiServer.Security.AccessLevel.NoAccess))
            .Do(call => savedFolder = call.Arg<IContent>());

        service.SiteUpdated(updatedSite, previousSite);

        var savedSettingsFolder = Assert.IsType<TestSettingsFolder>(savedFolder);
        Assert.Equal("NewName", savedSettingsFolder.Name);
        Assert.Equal("NewName", savedSettingsFolder.SiteId);
    }

    [Fact]
    public void SiteUpdated_WhenSiteNameDoesNotChange_DoesNotSaveOrClearCache()
    {
        var service = CreateService();
        service.GlobalSettingsRoot = CreateContentReference(10);
        var site = CreateWebsite("MySite");

        service.SiteUpdated(site, site);

        ContentRepository.DidNotReceive().GetChildren<SettingsFolder>(Arg.Any<ContentReference>());
        ContentRepository.DidNotReceive().Save(Arg.Any<IContent>(), Arg.Any<EPiServer.DataAccess.SaveAction>(), Arg.Any<EPiServer.Security.AccessLevel>());
        CacheManager.DidNotReceive().Remove(Arg.Any<string>());
    }

    [Fact]
    public void SiteUpdated_WhenFolderDoesNotExist_CreatesSiteFolder()
    {
        var service = CreateService();
        var rootRef = CreateContentReference(10);
        service.GlobalSettingsRoot = rootRef;

        var prevSite = CreateWebsite("OldName");
        var updatedSite = CreateWebsite("NewName");
        ContentRepository.GetChildren<SettingsFolder>(rootRef).Returns([]);
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

        service.SiteUpdated(updatedSite, prevSite);

        ContentRepository.Received().GetDefault<SettingsFolder>(Arg.Any<ContentReference>());
    }

    #endregion
}
