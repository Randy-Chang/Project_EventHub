using EventHub.Domain.Participants;
using EventHub.Domain.Quizzes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

internal sealed class ParticipantQuestionResultConfiguration : IEntityTypeConfiguration<ParticipantQuestionResult>
{
    public void Configure(EntityTypeBuilder<ParticipantQuestionResult> builder)
    {
        builder.ToTable("ParticipantQuestionResults");
        builder.HasKey(result => result.Id);
        builder.Property(result => result.CalculatedAtUtc)
            .HasConversion(
                value => value.ToUnixTimeMilliseconds(),
                value => DateTimeOffset.FromUnixTimeMilliseconds(value));

        builder.HasOne<EventHub.Domain.Events.Event>()
            .WithMany()
            .HasForeignKey(result => result.EventId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Quiz>()
            .WithMany()
            .HasForeignKey(result => result.QuizId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<QuizQuestionSession>()
            .WithMany()
            .HasForeignKey(result => result.QuestionSessionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Participant>()
            .WithMany()
            .HasForeignKey(result => result.ParticipantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(result => new { result.ParticipantId, result.QuestionSessionId }).IsUnique();
        builder.HasIndex(result => result.QuestionSessionId);
    }
}
