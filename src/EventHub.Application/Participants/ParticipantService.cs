using EventHub.Application.Abstractions;
using EventHub.Application.Events;
using EventHub.Domain.Common;
using EventHub.Domain.Participants;

namespace EventHub.Application.Participants;

public sealed class ParticipantService(
    IParticipantRepository participantRepository,
    IParticipantPresenceStore presenceStore,
    ICredentialService credentialService,
    EventService eventService,
    TimeProvider timeProvider)
{
    public async Task<JoinParticipantResult> JoinAsync(
        JoinParticipantCommand command,
        CancellationToken cancellationToken)
    {
        await eventService.EnsureJoinIsOpenAsync(command.EventId, cancellationToken);
        var sessionToken = string.IsNullOrWhiteSpace(command.SessionToken)
            ? credentialService.GenerateToken()
            : command.SessionToken;

        if (sessionToken.Length is < 32 or > 200)
        {
            throw new DomainValidationException("Participant session token 格式無效。");
        }

        var existing = await participantRepository.GetBySessionHashAsync(
            command.EventId,
            credentialService.HashToken(sessionToken),
            cancellationToken);

        if (existing is not null)
        {
            existing.MarkSeen(timeProvider.GetUtcNow());
            await participantRepository.UpdateAsync(existing, cancellationToken);
            return new JoinParticipantResult(ToSummary(existing, false), sessionToken, false);
        }

        var normalizedEmployeeNumber = NormalizeEmployeeNumber(command.EmployeeNumber);
        if (normalizedEmployeeNumber is not null)
        {
            var duplicate = await participantRepository.GetByEmployeeNumberAsync(
                command.EventId,
                normalizedEmployeeNumber,
                cancellationToken);

            if (duplicate is not null)
            {
                throw new DomainValidationException(
                    "此員工編號已加入活動；請使用原瀏覽器，或請 Host 協助恢復身份。");
            }
        }

        var participant = Participant.Create(
            command.EventId,
            command.Name,
            command.Nickname,
            command.EmployeeNumber,
            command.Department,
            command.TableNumber,
            credentialService.HashToken(sessionToken),
            timeProvider.GetUtcNow());

        await participantRepository.AddAsync(participant, cancellationToken);
        return new JoinParticipantResult(ToSummary(participant, false), sessionToken, true);
    }

    public async Task<ParticipantSummary> ValidateSessionAsync(
        Guid eventId,
        Guid participantId,
        string sessionToken,
        CancellationToken cancellationToken)
    {
        var participant = await GetValidatedParticipantAsync(
            eventId,
            participantId,
            sessionToken,
            cancellationToken);

        participant.MarkSeen(timeProvider.GetUtcNow());
        await participantRepository.UpdateAsync(participant, cancellationToken);
        return ToSummary(participant, presenceStore.GetOnlineParticipantIds(eventId).Contains(participant.Id));
    }

    public async Task<ParticipantSummary> ValidateSessionCredentialAsync(
        Guid eventId,
        Guid participantId,
        string sessionToken,
        CancellationToken cancellationToken)
    {
        var participant = await GetValidatedParticipantAsync(
            eventId,
            participantId,
            sessionToken,
            cancellationToken);
        return ToSummary(participant, presenceStore.GetOnlineParticipantIds(eventId).Contains(participant.Id));
    }

    public async Task<IReadOnlyList<ParticipantSummary>> ListForHostAsync(
        Guid eventId,
        string hostToken,
        CancellationToken cancellationToken)
    {
        if (!await eventService.IsHostAuthorizedAsync(eventId, hostToken, cancellationToken))
        {
            throw new UnauthorizedAccessException("Host credential 無效。");
        }

        var onlineIds = presenceStore.GetOnlineParticipantIds(eventId);
        var participants = await participantRepository.ListByEventAsync(eventId, cancellationToken);
        return participants.Select(participant => ToSummary(participant, onlineIds.Contains(participant.Id))).ToArray();
    }

    public async Task<int> GetParticipantCountAsync(
        Guid eventId,
        CancellationToken cancellationToken)
    {
        _ = await eventService.GetAsync(eventId, cancellationToken);
        return await participantRepository.CountByEventAsync(eventId, cancellationToken);
    }

    private static ParticipantSummary ToSummary(Participant participant, bool isOnline)
    {
        return new ParticipantSummary(
            participant.Id,
            participant.EventId,
            participant.Name,
            participant.DisplayName,
            participant.EmployeeNumber,
            participant.Department,
            participant.TableNumber,
            participant.IsCheckedIn,
            isOnline,
            participant.HasWon,
            participant.Score);
    }

    private async Task<Participant> GetValidatedParticipantAsync(
        Guid eventId,
        Guid participantId,
        string sessionToken,
        CancellationToken cancellationToken)
    {
        var participant = await participantRepository.GetByIdAsync(participantId, cancellationToken);
        if (participant is null ||
            participant.EventId != eventId ||
            !credentialService.Matches(sessionToken, participant.SessionCredentialHash))
        {
            throw new UnauthorizedAccessException("Participant credential 無效。");
        }

        return participant;
    }

    private static string? NormalizeEmployeeNumber(string? employeeNumber)
    {
        var normalized = employeeNumber?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized.ToUpperInvariant();
    }
}
