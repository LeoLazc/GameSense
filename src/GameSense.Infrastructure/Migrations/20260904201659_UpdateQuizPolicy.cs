using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameSense.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuizPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuizSessions_UserId",
                table: "QuizSessions");

            migrationBuilder.AddColumn<decimal>(
                name: "ExpertiseScore",
                table: "Users",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE Questions
SET QuestionWeight = CASE Difficulty
    WHEN 1 THEN 1.00
    WHEN 2 THEN 1.50
    WHEN 3 THEN 2.00
    ELSE QuestionWeight
END;");

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessions_UserId",
                table: "QuizSessions",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuizSessions_UserId",
                table: "QuizSessions");

            migrationBuilder.DropColumn(
                name: "ExpertiseScore",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessions_UserId",
                table: "QuizSessions",
                column: "UserId");
        }
    }
}
