using EventHub.Domain.Participants;
using Microsoft.EntityFrameworkCore;
using DomainEvent = EventHub.Domain.Events.Event;

namespace EventHub.Infrastructure.Persistence;

public sealed class EventHubDbContext(DbContextOptions<EventHubDbContext> options) : DbContext(options)
{
    public DbSet<DomainEvent> Events => Set<DomainEvent>();

    public DbSet<Participant> Participants => Set<Participant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventHubDbContext).Assembly);
    }
}
