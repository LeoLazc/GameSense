using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameSense.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameGameExternalId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Games_IgdbId",
                table: "Games",
                newName: "IX_Games_ExternalId");

            migrationBuilder.RenameColumn(
                name: "IgdbId",
                table: "Games",
                newName: "ExternalId");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Games_ExternalId",
                table: "Games",
                newName: "IX_Games_IgdbId");

            migrationBuilder.RenameColumn(
                name: "ExternalId",
                table: "Games",
                newName: "IgdbId");

        }
    }
}
