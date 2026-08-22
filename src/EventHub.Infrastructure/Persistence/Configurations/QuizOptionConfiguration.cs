using EventHub.Domain.Quizzes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

internal sealed class QuizOptionConfiguration : IEntityTypeConfiguration<QuizOption>
{
    public void Configure(EntityTypeBuilder<QuizOption> builder)
    {
        builder.ToTable("QuizOptions");
        builder.HasKey(option => option.Id);
        builder.Property(option => option.Text).HasMaxLength(200).IsRequired();
        builder.HasIndex(option => new { option.QuizQuestionId, option.Order }).IsUnique();
    }
}
