using EventHub.Application.Abstractions;
using EventHub.Domain.Common;
using EventHub.Domain.Events;
using DomainEvent = EventHub.Domain.Events.Event;

namespace EventHub.Application.Events;

public sealed class EventService(
    IEventRepository eventRepository,
    ICredentialService credentialService,
    IEventJoinCodeGenerator joinCodeGenerator,
    EventJoinUrlBuilder joinUrlBuilder,
    TimeProvider timeProvider)
{
    private const int JoinCodeGenerationAttemptLimit = 10;

    public async Task<CreateEventResult> CreateAsync(
        CreateEventCommand command,
        CancellationToken cancellationToken)
    {
        var hostToken = credentialService.GenerateToken();
        for (var attempt = 0; attempt < JoinCodeGenerationAttemptLimit; attempt++)
        {
            var joinCode = EventJoinCode.Normalize(joinCodeGenerator.Generate());
            if (await eventRepository.JoinCodeExistsAsync(joinCode, cancellationToken))
            {
                continue;
            }

            var eventItem = DomainEvent.Create(
                command.Name,
                joinCode,
                command.EventDateUtc.ToUniversalTime(),
                credentialService.HashToken(hostToken),
                timeProvider.GetUtcNow());

            if (await eventRepository.TryAddAsync(eventItem, cancellationToken))
            {
                return new CreateEventResult(
                    ToSummary(eventItem),
                    hostToken,
                    ToJoinInfo(eventItem));
            }
        }

        throw new EventJoinCodeGenerationException();
    }

    public async Task<EventSummary> GetAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var eventItem = await GetRequiredAsync(eventId, cancellationToken);
        return ToSummary(eventItem);
    }

    public async Task<EventJoinInfo> GetJoinInfoForHostAsync(
        Guid eventId,
        string hostToken,
        CancellationToken cancellationToken)
    {
        var eventItem = await GetRequiredAsync(eventId, cancellationToken);
        if (!credentialService.Matches(hostToken, eventItem.HostCredentialHash))
        {
            throw new UnauthorizedAccessException("Host credential 無效。");
        }

        return ToJoinInfo(eventItem);
    }

    public async Task<EventJoinInfo> GetJoinInfoAsync(
        Guid eventId,
        CancellationToken cancellationToken)
    {
        return ToJoinInfo(await GetRequiredAsync(eventId, cancellationToken));
    }

    public async Task<EventJoinInfo> ResolveJoinCodeAsync(
        string joinCode,
        CancellationToken cancellationToken)
    {
        var normalizedCode = EventJoinCode.Normalize(joinCode);
        var eventItem = await eventRepository.GetByJoinCodeAsync(normalizedCode, cancellationToken)
            ?? throw new KeyNotFoundException("找不到此活動，請確認 QR Code 或加入網址是否正確。");
        return ToJoinInfo(eventItem);
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
        if (!eventItem.IsJoinOpen || eventItem.State is EventState.Ended or EventState.Cancelled)
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
            eventItem.JoinCode,
            eventItem.EventDateUtc,
            eventItem.State,
            eventItem.IsJoinOpen);
    }

    private EventJoinInfo ToJoinInfo(DomainEvent eventItem)
    {
        return new EventJoinInfo(
            eventItem.Id,
            eventItem.Name,
            eventItem.JoinCode,
            joinUrlBuilder.Build(eventItem.JoinCode),
            eventItem.IsJoinOpen && eventItem.State is not (EventState.Ended or EventState.Cancelled),
            joinUrlBuilder.IsLoopback);
    }
}
