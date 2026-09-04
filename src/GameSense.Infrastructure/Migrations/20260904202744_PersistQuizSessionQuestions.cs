using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameSense.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PersistQuizSessionQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuizAnswers_QuizSessionId",
                table: "QuizAnswers");

            migrationBuilder.CreateTable(
                name: "QuizSessionQuestions",
                columns: table => new
                {
                    QuizSessionId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizSessionQuestions", x => new { x.QuizSessionId, x.QuestionId });
                    table.ForeignKey(
                        name: "FK_QuizSessionQuestions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizSessionQuestions_QuizSessions_QuizSessionId",
                        column: x => x.QuizSessionId,
                        principalTable: "QuizSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswers_QuizSessionId_QuestionId",
                table: "QuizAnswers",
                columns: new[] { "QuizSessionId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessionQuestions_QuestionId",
                table: "QuizSessionQuestions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessionQuestions_QuizSessionId_Order",
                table: "QuizSessionQuestions",
                columns: new[] { "QuizSessionId", "Order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizSessionQuestions");

            migrationBuilder.DropIndex(
                name: "IX_QuizAnswers_QuizSessionId_QuestionId",
                table: "QuizAnswers");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswers_QuizSessionId",
                table: "QuizAnswers",
                column: "QuizSessionId");
        }
    }
}
