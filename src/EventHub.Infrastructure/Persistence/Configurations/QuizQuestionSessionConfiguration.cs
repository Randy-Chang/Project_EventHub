using EventHub.Domain.Quizzes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

internal sealed class QuizQuestionSessionConfiguration : IEntityTypeConfiguration<QuizQuestionSession>
{
    public void Configure(EntityTypeBuilder<QuizQuestionSession> builder)
    {
        builder.ToTable("QuizQuestionSessions");
        builder.HasKey(session => session.Id);
        builder.Property(session => session.State).HasConversion<string>().HasMaxLength(20).IsRequired();
        ConfigureNullableDate(builder.Property(session => session.StartedAtUtc));
        ConfigureNullableDate(builder.Property(session => session.AnswerDeadlineUtc));
        ConfigureNullableDate(builder.Property(session => session.ClosedAtUtc));
        ConfigureNullableDate(builder.Property(session => session.RevealedAtUtc));
        builder.Property(session => session.Version).IsConcurrencyToken();

        builder.HasOne<EventHub.Domain.Events.Event>()
            .WithMany()
            .HasForeignKey(session => session.EventId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Quiz>()
            .WithMany()
            .HasForeignKey(session => session.QuizId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<QuizQuestion>()
            .WithMany()
            .HasForeignKey(session => session.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(session => new { session.EventId, session.StartedAtUtc });
        builder.HasIndex(session => session.EventId)
            .IsUnique()
            .HasFilter("State = 'Open'");
    }

    private static void ConfigureNullableDate(
        PropertyBuilder<DateTimeOffset?> propertyBuilder)
    {
        propertyBuilder.HasConversion(
            value => value.HasValue ? value.Value.ToUnixTimeMilliseconds() : (long?)null,
            value => value.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(value.Value) : null);
    }
}
