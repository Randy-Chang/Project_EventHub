using EventHub.Application.Abstractions;
using EventHub.Domain.Common;
using DomainEvent = EventHub.Domain.Events.Event;

namespace EventHub.Application.Events;

public sealed class EventService(
    IEventRepository eventRepository,
    ICredentialService credentialService,
    TimeProvider timeProvider)
{
    public async Task<CreateEventResult> CreateAsync(
        CreateEventCommand command,
        CancellationToken cancellationToken)
    {
        var hostToken = credentialService.GenerateToken();
        var eventItem = DomainEvent.Create(
            command.Name,
            command.EventDateUtc.ToUniversalTime(),
            credentialService.HashToken(hostToken),
            timeProvider.GetUtcNow());

        await eventRepository.AddAsync(eventItem, cancellationToken);

        return new CreateEventResult(ToSummary(eventItem), hostToken);
    }

    public async Task<EventSummary> GetAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var eventItem = await GetRequiredAsync(eventId, cancellationToken);
        return ToSummary(eventItem);
    }

    public async Task<bool> IsHostAuthorizedAsync(
        Guid eventId,
        string hostToken,
        CancellationToken cancellationToken)
    {
        var eventItem = await eventRepository.GetByIdAsync(eventId, cancellationToken);
        return eventItem is not null && credentialService.Matches(hostToken, eventItem.HostCredentialHash);
    }

    public async Task EnsureJoinIsOpenAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var eventItem = await GetRequiredAsync(eventId, cancellationToken);
        if (!eventItem.IsJoinOpen)
        {
            throw new DomainValidationException("此活動目前未開放加入。");
        }
    }

    private async Task<DomainEvent> GetRequiredAsync(Guid eventId, CancellationToken cancellationToken)
    {
        return await eventRepository.GetByIdAsync(eventId, cancellationToken)
            ?? throw new KeyNotFoundException("找不到指定的活動。");
    }

    private static EventSummary ToSummary(DomainEvent eventItem)
    {
        return new EventSummary(
            eventItem.Id,
            eventItem.Name,
            eventItem.EventDateUtc,
            eventItem.State,
            eventItem.IsJoinOpen);
    }
}
