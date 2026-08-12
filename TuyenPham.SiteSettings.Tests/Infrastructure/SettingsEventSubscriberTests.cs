using EPiServer.Applications;
using EPiServer.Core;
using NSubstitute;
using TuyenPham.SiteSettings.Infrastructure;
using TuyenPham.SiteSettings.Services;

namespace TuyenPham.SiteSettings.Tests.Infrastructure;

public class SettingsEventSubscriberTests
{
    private readonly ISettingsService _settingsService = Substitute.For<ISettingsService>();

    [Fact]
    public async Task HandleAsync_ForCreatedSite_CreatesSettings()
    {
        var application = new Website("new-site", new ContentReference(1));
        var subscriber = new SettingsEventSubscriber(_settingsService);

        await subscriber.HandleAsync(new ApplicationCreatedEvent(application), null!, TestContext.Current.CancellationToken);

        _settingsService.Received().SiteCreated(application);
    }

    [Fact]
    public async Task HandleAsync_ForDeletedSite_RemovesSettings()
    {
        var application = new Website("deleted-site", new ContentReference(1));
        var subscriber = new SettingsEventSubscriber(_settingsService);

        await subscriber.HandleAsync(new ApplicationDeletedEvent(application), null!, TestContext.Current.CancellationToken);

        _settingsService.Received().SiteDeleted(application);
    }

    [Fact]
    public async Task HandleAsync_ForUpdatedSite_RenamesSettings()
    {
        var previous = new Website("old-site", new ContentReference(1));
        var updated = new Website("new-site", new ContentReference(1));
        var subscriber = new SettingsEventSubscriber(_settingsService);

        await subscriber.HandleAsync(new ApplicationUpdatedEvent(updated, previous), null!, TestContext.Current.CancellationToken);

        _settingsService.Received().SiteUpdated(updated, previous);
    }
}