using EventHub.Domain.Participants;
using EventHub.Domain.Quizzes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

internal sealed class ParticipantQuizScoreConfiguration : IEntityTypeConfiguration<ParticipantQuizScore>
{
    public void Configure(EntityTypeBuilder<ParticipantQuizScore> builder)
    {
        builder.ToTable("ParticipantQuizScores");
        builder.HasKey(score => score.Id);
        builder.Property(score => score.UpdatedAtUtc)
            .HasConversion(
                value => value.ToUnixTimeMilliseconds(),
                value => DateTimeOffset.FromUnixTimeMilliseconds(value));

        builder.HasOne<EventHub.Domain.Events.Event>()
            .WithMany()
            .HasForeignKey(score => score.EventId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Quiz>()
            .WithMany()
            .HasForeignKey(score => score.QuizId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Participant>()
            .WithMany()
            .HasForeignKey(score => score.ParticipantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(score => new { score.ParticipantId, score.QuizId }).IsUnique();
        builder.HasIndex(score => new { score.EventId, score.QuizId, score.TotalScore });
    }
}
