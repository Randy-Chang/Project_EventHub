using EventHub.Application.Abstractions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using DomainEvent = EventHub.Domain.Events.Event;

namespace EventHub.Infrastructure.Persistence.Repositories;

public sealed class EventRepository(EventHubDbContext dbContext) : IEventRepository
{
    public Task<DomainEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken)
    {
        return dbContext.Events.SingleOrDefaultAsync(eventItem => eventItem.Id == eventId, cancellationToken);
    }

    public Task<DomainEvent?> GetByJoinCodeAsync(
        string normalizedJoinCode,
        CancellationToken cancellationToken)
    {
        return dbContext.Events.SingleOrDefaultAsync(
            eventItem => eventItem.JoinCode == normalizedJoinCode,
            cancellationToken);
    }

    public Task<bool> JoinCodeExistsAsync(
        string normalizedJoinCode,
        CancellationToken cancellationToken)
    {
        return dbContext.Events.AnyAsync(
            eventItem => eventItem.JoinCode == normalizedJoinCode,
            cancellationToken);
    }

    public async Task<bool> TryAddAsync(DomainEvent eventItem, CancellationToken cancellationToken)
    {
        dbContext.Events.Add(eventItem);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            dbContext.Entry(eventItem).State = EntityState.Detached;
            return false;
        }
    }

    public async Task UpdateAsync(DomainEvent eventItem, CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await dbContext.Entry(eventItem).ReloadAsync(cancellationToken);
            throw;
        }
    }
}
