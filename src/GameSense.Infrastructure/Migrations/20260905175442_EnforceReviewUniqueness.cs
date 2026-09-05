using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameSense.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnforceReviewUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Games_ExternalId",
                table: "Games");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId_GameId",
                table: "Reviews",
                columns: new[] { "UserId", "GameId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_CatalogProvider_ExternalId",
                table: "Games",
                columns: new[] { "CatalogProvider", "ExternalId" },
                unique: true,
                filter: "[CatalogProvider] IS NOT NULL AND [ExternalId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId_GameId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Games_CatalogProvider_ExternalId",
                table: "Games");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_ExternalId",
                table: "Games",
                column: "ExternalId",
                unique: true,
                filter: "[ExternalId] IS NOT NULL");
        }
    }
}
