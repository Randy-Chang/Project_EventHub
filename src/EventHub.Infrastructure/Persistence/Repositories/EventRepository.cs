using EventHub.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using DomainEvent = EventHub.Domain.Events.Event;

namespace EventHub.Infrastructure.Persistence.Repositories;

public sealed class EventRepository(EventHubDbContext dbContext) : IEventRepository
{
    public Task<DomainEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken)
    {
        return dbContext.Events.SingleOrDefaultAsync(eventItem => eventItem.Id == eventId, cancellationToken);
    }

    public async Task AddAsync(DomainEvent eventItem, CancellationToken cancellationToken)
    {
        dbContext.Events.Add(eventItem);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
