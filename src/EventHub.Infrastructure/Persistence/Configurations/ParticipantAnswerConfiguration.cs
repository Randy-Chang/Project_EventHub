using EventHub.Domain.Participants;
using EventHub.Domain.Quizzes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

internal sealed class ParticipantAnswerConfiguration : IEntityTypeConfiguration<ParticipantAnswer>
{
    public void Configure(EntityTypeBuilder<ParticipantAnswer> builder)
    {
        builder.ToTable("ParticipantAnswers");
        builder.HasKey(answer => answer.Id);
        builder.Property(answer => answer.SubmittedAtUtc)
            .HasConversion(
                value => value.ToUnixTimeMilliseconds(),
                value => DateTimeOffset.FromUnixTimeMilliseconds(value));

        builder.HasOne<EventHub.Domain.Events.Event>()
            .WithMany()
            .HasForeignKey(answer => answer.EventId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<QuizQuestionSession>()
            .WithMany()
            .HasForeignKey(answer => answer.QuestionSessionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Participant>()
            .WithMany()
            .HasForeignKey(answer => answer.ParticipantId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<QuizOption>()
            .WithMany()
            .HasForeignKey(answer => answer.SelectedOptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(answer => new { answer.ParticipantId, answer.QuestionSessionId }).IsUnique();
        builder.HasIndex(answer => answer.QuestionSessionId);
    }
}
