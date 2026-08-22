using EventHub.Domain.Participants;

namespace EventHub.Application.Abstractions;

public interface IParticipantRepository
{
    Task<Participant?> GetByIdAsync(Guid participantId, CancellationToken cancellationToken);

    Task<Participant?> GetBySessionHashAsync(
        Guid eventId,
        string sessionCredentialHash,
        CancellationToken cancellationToken);

    Task<Participant?> GetByEmployeeNumberAsync(
        Guid eventId,
        string normalizedEmployeeNumber,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Participant>> ListByEventAsync(Guid eventId, CancellationToken cancellationToken);

    Task AddAsync(Participant participant, CancellationToken cancellationToken);

    Task UpdateAsync(Participant participant, CancellationToken cancellationToken);
}
