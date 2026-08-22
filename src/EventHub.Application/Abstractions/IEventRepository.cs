using EventHub.Domain.Events;

namespace EventHub.Application.Abstractions;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken);

    Task<Event?> GetByJoinCodeAsync(string normalizedJoinCode, CancellationToken cancellationToken);

    Task<bool> JoinCodeExistsAsync(string normalizedJoinCode, CancellationToken cancellationToken);

    Task<bool> TryAddAsync(Event eventItem, CancellationToken cancellationToken);

    Task UpdateAsync(Event eventItem, CancellationToken cancellationToken);
}
