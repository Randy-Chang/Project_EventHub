using EventHub.Application.Abstractions;
using EventHub.Domain.Common;
using EventHub.Domain.Participants;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence.Repositories;

public sealed class ParticipantRepository(EventHubDbContext dbContext) : IParticipantRepository
{
    public Task<Participant?> GetByIdAsync(Guid participantId, CancellationToken cancellationToken)
    {
        return dbContext.Participants.SingleOrDefaultAsync(
            participant => participant.Id == participantId,
            cancellationToken);
    }

    public Task<Participant?> GetBySessionHashAsync(
        Guid eventId,
        string sessionCredentialHash,
        CancellationToken cancellationToken)
    {
        return dbContext.Participants.SingleOrDefaultAsync(
            participant => participant.EventId == eventId &&
                participant.SessionCredentialHash == sessionCredentialHash,
            cancellationToken);
    }

    public Task<Participant?> GetByEmployeeNumberAsync(
        Guid eventId,
        string normalizedEmployeeNumber,
        CancellationToken cancellationToken)
    {
        return dbContext.Participants.SingleOrDefaultAsync(
            participant => participant.EventId == eventId &&
                participant.NormalizedEmployeeNumber == normalizedEmployeeNumber,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Participant>> ListByEventAsync(
        Guid eventId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Participants
            .AsNoTracking()
            .Where(participant => participant.EventId == eventId)
            .OrderBy(participant => participant.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Participant participant, CancellationToken cancellationToken)
    {
        dbContext.Participants.Add(participant);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            dbContext.Entry(participant).State = EntityState.Detached;
            throw new DomainValidationException("Participant 身份已存在，請重試加入。");
        }
    }

    public Task UpdateAsync(Participant participant, CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
