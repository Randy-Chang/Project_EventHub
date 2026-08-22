using EventHub.Domain.Quizzes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

internal sealed class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.ToTable("QuizQuestions");
        builder.HasKey(question => question.Id);
        builder.Property(question => question.Text).HasMaxLength(500).IsRequired();
        builder.Property(question => question.AnswerDuration)
            .HasConversion(value => value.Ticks, value => TimeSpan.FromTicks(value));
        builder.Property(question => question.CorrectOptionId).IsRequired();

        builder.HasOne<Quiz>()
            .WithMany()
            .HasForeignKey(question => question.QuizId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(question => question.Options)
            .WithOne()
            .HasForeignKey(option => option.QuizQuestionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(question => question.Options).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(question => new { question.QuizId, question.Order }).IsUnique();
    }
}
