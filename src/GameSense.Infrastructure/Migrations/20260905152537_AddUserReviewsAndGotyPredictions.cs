using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameSense.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserReviewsAndGotyPredictions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The old table stored AI-generated franchise reviews and cannot be safely transformed
            // into user-authored game reviews. Existing review rows are intentionally discarded.
            migrationBuilder.DropTable(name: "Reviews");

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey("FK_Reviews_Games_GameId", x => x.GameId, "Games", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_Reviews_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateIndex(name: "IX_Reviews_GameId", table: "Reviews", column: "GameId");
            migrationBuilder.CreateIndex(name: "IX_Reviews_UserId", table: "Reviews", column: "UserId");

            migrationBuilder.CreateTable(
                name: "GotyPredictions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    GotyGameId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GotyPredictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GotyPredictions_Games_GotyGameId",
                        column: x => x.GotyGameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GotyPredictions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GotyNominees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PredictionId = table.Column<int>(type: "int", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GotyNominees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GotyNominees_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GotyNominees_GotyPredictions_PredictionId",
                        column: x => x.PredictionId,
                        principalTable: "GotyPredictions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GotyNominees_GameId",
                table: "GotyNominees",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GotyNominees_PredictionId_GameId",
                table: "GotyNominees",
                columns: new[] { "PredictionId", "GameId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GotyNominees_PredictionId_Order",
                table: "GotyNominees",
                columns: new[] { "PredictionId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GotyPredictions_GotyGameId",
                table: "GotyPredictions",
                column: "GotyGameId");

            migrationBuilder.CreateIndex(
                name: "IX_GotyPredictions_UserId_Year",
                table: "GotyPredictions",
                columns: new[] { "UserId", "Year" },
                unique: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Reviews");

            migrationBuilder.DropTable(
                name: "GotyNominees");

            migrationBuilder.DropTable(
                name: "GotyPredictions");

        }
    }
}
