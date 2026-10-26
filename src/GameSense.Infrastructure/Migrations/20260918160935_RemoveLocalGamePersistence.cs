using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameSense.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLocalGamePersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GotyNominees_Games_GameId",
                table: "GotyNominees");

            migrationBuilder.DropForeignKey(
                name: "FK_GotyPredictions_Games_GotyGameId",
                table: "GotyPredictions");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Games_GameId",
                table: "Reviews");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Franchises");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_GameId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId_GameId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_GotyPredictions_GotyGameId",
                table: "GotyPredictions");

            migrationBuilder.DropIndex(
                name: "IX_GotyNominees_GameId",
                table: "GotyNominees");

            migrationBuilder.DropIndex(
                name: "IX_GotyNominees_PredictionId_GameId",
                table: "GotyNominees");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "GotyGameId",
                table: "GotyPredictions");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "GotyNominees");

            migrationBuilder.AddColumn<string>(
                name: "RawgGameId",
                table: "Reviews",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GotyRawgGameId",
                table: "GotyPredictions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RawgGameId",
                table: "GotyNominees",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId_RawgGameId",
                table: "Reviews",
                columns: new[] { "UserId", "RawgGameId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GotyNominees_PredictionId_RawgGameId",
                table: "GotyNominees",
                columns: new[] { "PredictionId", "RawgGameId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId_RawgGameId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_GotyNominees_PredictionId_RawgGameId",
                table: "GotyNominees");

            migrationBuilder.DropColumn(
                name: "RawgGameId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "GotyRawgGameId",
                table: "GotyPredictions");

            migrationBuilder.DropColumn(
                name: "RawgGameId",
                table: "GotyNominees");

            migrationBuilder.AddColumn<int>(
                name: "GameId",
                table: "Reviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GotyGameId",
                table: "GotyPredictions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GameId",
                table: "GotyNominees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Franchises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Franchises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FranchiseId = table.Column<int>(type: "int", nullable: true),
                    BackgroundImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CatalogProvider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    ExternalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FranchiseExternalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FranchiseName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastSyncedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReleaseYear = table.Column<int>(type: "int", nullable: false),
                    ReleasedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Games_Franchises_FranchiseId",
                        column: x => x.FranchiseId,
                        principalTable: "Franchises",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_GameId",
                table: "Reviews",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId_GameId",
                table: "Reviews",
                columns: new[] { "UserId", "GameId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GotyPredictions_GotyGameId",
                table: "GotyPredictions",
                column: "GotyGameId");

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
                name: "IX_Games_CatalogProvider_ExternalId",
                table: "Games",
                columns: new[] { "CatalogProvider", "ExternalId" },
                unique: true,
                filter: "[CatalogProvider] IS NOT NULL AND [ExternalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Games_FranchiseId",
                table: "Games",
                column: "FranchiseId");

            migrationBuilder.AddForeignKey(
                name: "FK_GotyNominees_Games_GameId",
                table: "GotyNominees",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GotyPredictions_Games_GotyGameId",
                table: "GotyPredictions",
                column: "GotyGameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Games_GameId",
                table: "Reviews",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
