using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizQuestionModeAndExplanation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Explanation",
                table: "QuizQuestions",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mode",
                table: "QuizQuestions",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "Scored");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Explanation",
                table: "QuizQuestions");

            migrationBuilder.DropColumn(
                name: "Mode",
                table: "QuizQuestions");
        }
    }
}
