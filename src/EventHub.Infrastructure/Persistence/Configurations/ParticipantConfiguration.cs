using EventHub.Domain.Participants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

internal sealed class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
{
    public void Configure(EntityTypeBuilder<Participant> builder)
    {
        builder.ToTable("Participants");
        builder.HasKey(participant => participant.Id);
        builder.Property(participant => participant.Name).HasMaxLength(100).IsRequired();
        builder.Property(participant => participant.Nickname).HasMaxLength(100);
        builder.Property(participant => participant.EmployeeNumber).HasMaxLength(50);
        builder.Property(participant => participant.NormalizedEmployeeNumber).HasMaxLength(50);
        builder.Property(participant => participant.Department).HasMaxLength(100);
        builder.Property(participant => participant.TableNumber).HasMaxLength(50);
        builder.Property(participant => participant.SessionCredentialHash).HasMaxLength(64).IsRequired();
        builder.Property(participant => participant.LastSeenAtUtc)
            .HasConversion(
                value => value.ToUnixTimeMilliseconds(),
                value => DateTimeOffset.FromUnixTimeMilliseconds(value));
        builder.Property(participant => participant.CreatedAtUtc)
            .HasConversion(
                value => value.ToUnixTimeMilliseconds(),
                value => DateTimeOffset.FromUnixTimeMilliseconds(value));
        builder.Property(participant => participant.Version).IsConcurrencyToken();
        builder.Ignore(participant => participant.DisplayName);

        builder.HasOne<EventHub.Domain.Events.Event>()
            .WithMany()
            .HasForeignKey(participant => participant.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(participant => new { participant.EventId, participant.SessionCredentialHash })
            .IsUnique();
        builder.HasIndex(participant => new { participant.EventId, participant.NormalizedEmployeeNumber })
            .IsUnique()
            .HasFilter("NormalizedEmployeeNumber IS NOT NULL");
        builder.HasIndex(participant => new { participant.EventId, participant.CreatedAtUtc });
    }
}
