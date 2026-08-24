using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEventLifecycleAndHostRecovery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Events SET State = 'Active' WHERE State = 'Running';");
            migrationBuilder.Sql("UPDATE Events SET State = 'Completed', IsJoinOpen = 0 WHERE State IN ('Ended', 'Cancelled');");
            migrationBuilder.Sql("UPDATE Events SET State = 'Active' WHERE State = 'Draft' AND EXISTS (SELECT 1 FROM QuizQuestionSessions WHERE QuizQuestionSessions.EventId = Events.Id);");
            migrationBuilder.Sql("UPDATE Events SET State = 'Ready' WHERE State = 'Draft' AND (EXISTS (SELECT 1 FROM Participants WHERE Participants.EventId = Events.Id) OR EXISTS (SELECT 1 FROM Quizzes WHERE Quizzes.EventId = Events.Id));");
            migrationBuilder.Sql("UPDATE Events SET IsJoinOpen = 0 WHERE State IN ('Draft', 'Completed');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Events SET State = 'Running' WHERE State = 'Active';");
            migrationBuilder.Sql("UPDATE Events SET State = 'Ended' WHERE State = 'Completed';");
        }
    }
}
