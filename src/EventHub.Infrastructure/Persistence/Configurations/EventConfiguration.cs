using EventHub.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainEvent = EventHub.Domain.Events.Event;

namespace EventHub.Infrastructure.Persistence.Configurations;

internal sealed class EventConfiguration : IEntityTypeConfiguration<DomainEvent>
{
    public void Configure(EntityTypeBuilder<DomainEvent> builder)
    {
        builder.ToTable("Events");
        builder.HasKey(eventItem => eventItem.Id);
        builder.Property(eventItem => eventItem.Name).HasMaxLength(200).IsRequired();
        builder.Property(eventItem => eventItem.EventDateUtc)
            .HasConversion(
                value => value.ToUnixTimeMilliseconds(),
                value => DateTimeOffset.FromUnixTimeMilliseconds(value));
        builder.Property(eventItem => eventItem.State).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(eventItem => eventItem.HostCredentialHash).HasMaxLength(64).IsRequired();
        builder.Property(eventItem => eventItem.CreatedAtUtc)
            .HasConversion(
                value => value.ToUnixTimeMilliseconds(),
                value => DateTimeOffset.FromUnixTimeMilliseconds(value));
        builder.Property(eventItem => eventItem.Version).IsConcurrencyToken();
        builder.HasIndex(eventItem => eventItem.EventDateUtc);
    }
}
