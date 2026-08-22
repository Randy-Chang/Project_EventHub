using EventHub.Domain.Quizzes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

internal sealed class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.ToTable("Quizzes");
        builder.HasKey(quiz => quiz.Id);
        builder.Property(quiz => quiz.Title).HasMaxLength(200).UseCollation("NOCASE").IsRequired();
        builder.Property(quiz => quiz.CreatedAtUtc)
            .HasConversion(
                value => value.ToUnixTimeMilliseconds(),
                value => DateTimeOffset.FromUnixTimeMilliseconds(value));

        builder.HasOne<EventHub.Domain.Events.Event>()
            .WithMany()
            .HasForeignKey(quiz => quiz.EventId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(quiz => new { quiz.EventId, quiz.Title }).IsUnique();
    }
}
