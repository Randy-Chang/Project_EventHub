using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using EventHub.Infrastructure.Persistence;

#nullable disable

namespace EventHub.Infrastructure.Persistence.Migrations;

[DbContext(typeof(EventHubDbContext))]
[Migration("20260822090000_AddQuestionBankImport")]
public partial class AddQuestionBankImport : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Quizzes_EventId", table: "Quizzes");
        migrationBuilder.AddColumn<string>(
            name: "Category",
            table: "QuizQuestions",
            type: "TEXT",
            maxLength: 100,
            nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "Difficulty",
            table: "QuizQuestions",
            type: "TEXT",
            maxLength: 20,
            nullable: false,
            defaultValue: "Medium");
        migrationBuilder.AddColumn<string>(
            name: "QuestionKey",
            table: "QuizQuestions",
            type: "TEXT",
            maxLength: 50,
            nullable: false,
            defaultValue: "");
        migrationBuilder.Sql(
            "UPDATE QuizQuestions SET QuestionKey = 'MANUAL-' || replace(Id, '-', '') WHERE QuestionKey = ''; ");
        migrationBuilder.Sql(
            "CREATE UNIQUE INDEX IX_Quizzes_EventId_Title ON Quizzes(EventId, Title COLLATE NOCASE);");
        migrationBuilder.Sql(
            "CREATE UNIQUE INDEX IX_QuizQuestions_QuizId_QuestionKey ON QuizQuestions(QuizId, QuestionKey COLLATE NOCASE);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Quizzes_EventId_Title", table: "Quizzes");
        migrationBuilder.DropIndex(name: "IX_QuizQuestions_QuizId_QuestionKey", table: "QuizQuestions");
        migrationBuilder.DropColumn(name: "Category", table: "QuizQuestions");
        migrationBuilder.DropColumn(name: "Difficulty", table: "QuizQuestions");
        migrationBuilder.DropColumn(name: "QuestionKey", table: "QuizQuestions");
        migrationBuilder.CreateIndex(name: "IX_Quizzes_EventId", table: "Quizzes", column: "EventId");
    }
}
