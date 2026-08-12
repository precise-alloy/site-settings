using EPiServer.Applications;
using EPiServer.Events;
using TuyenPham.SiteSettings.Services;
using System.Threading;
using System.Threading.Tasks;

namespace TuyenPham.SiteSettings.Infrastructure;

/// <summary>
/// Bridges CMS lifecycle events to the settings service.
/// </summary>
public sealed class SettingsEventSubscriber(ISettingsService settingsService)
    : IEventSubscriber<ApplicationCreatedEvent>,
        IEventSubscriber<ApplicationDeletedEvent>,
        IEventSubscriber<ApplicationUpdatedEvent>
{
    /// <inheritdoc />
    public Task HandleAsync(
        ApplicationCreatedEvent eventData,
        EventContext context,
        CancellationToken cancellationToken = default)
    {
        settingsService.SiteCreated(eventData.Application);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task HandleAsync(
        ApplicationDeletedEvent eventData,
        EventContext context,
        CancellationToken cancellationToken = default)
    {
        settingsService.SiteDeleted(eventData.Application);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task HandleAsync(
        ApplicationUpdatedEvent eventData,
        EventContext context,
        CancellationToken cancellationToken = default)
    {
        settingsService.SiteUpdated(eventData.Application, eventData.PreviousApplication);
        return Task.CompletedTask;
    }
}