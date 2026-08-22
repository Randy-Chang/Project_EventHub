using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizScoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParticipantQuestionResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuizId = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuestionSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: false),
                    BaseScore = table.Column<int>(type: "INTEGER", nullable: false),
                    SpeedBonus = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    ElapsedTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    CalculatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantQuestionResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParticipantQuestionResults_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParticipantQuestionResults_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParticipantQuestionResults_QuizQuestionSessions_QuestionSessionId",
                        column: x => x.QuestionSessionId,
                        principalTable: "QuizQuestionSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParticipantQuestionResults_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantQuizScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuizId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TotalScore = table.Column<int>(type: "INTEGER", nullable: false),
                    CorrectCount = table.Column<int>(type: "INTEGER", nullable: false),
                    AnsweredCount = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantQuizScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParticipantQuizScores_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParticipantQuizScores_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParticipantQuizScores_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantQuestionResults_EventId",
                table: "ParticipantQuestionResults",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantQuestionResults_ParticipantId_QuestionSessionId",
                table: "ParticipantQuestionResults",
                columns: new[] { "ParticipantId", "QuestionSessionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantQuestionResults_QuestionSessionId",
                table: "ParticipantQuestionResults",
                column: "QuestionSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantQuestionResults_QuizId",
                table: "ParticipantQuestionResults",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantQuizScores_EventId_QuizId_TotalScore",
                table: "ParticipantQuizScores",
                columns: new[] { "EventId", "QuizId", "TotalScore" });

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantQuizScores_ParticipantId_QuizId",
                table: "ParticipantQuizScores",
                columns: new[] { "ParticipantId", "QuizId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantQuizScores_QuizId",
                table: "ParticipantQuizScores",
                column: "QuizId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParticipantQuestionResults");

            migrationBuilder.DropTable(
                name: "ParticipantQuizScores");
        }
    }
}
