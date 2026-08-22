using EventHub.Domain.Events;

namespace EventHub.Application.Abstractions;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken);

    Task AddAsync(Event eventItem, CancellationToken cancellationToken);
}
