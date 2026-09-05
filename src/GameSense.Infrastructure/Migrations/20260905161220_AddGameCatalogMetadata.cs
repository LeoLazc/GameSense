using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameSense.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGameCatalogMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Franchises_FranchiseId",
                table: "Games");

            migrationBuilder.AlterColumn<int>(
                name: "FranchiseId",
                table: "Games",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundImageUrl",
                table: "Games",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CatalogProvider",
                table: "Games",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                table: "Games",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Games",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FranchiseExternalId",
                table: "Games",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FranchiseName",
                table: "Games",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IgdbId",
                table: "Games",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "Games",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReleasedAt",
                table: "Games",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Games",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "Games",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_IgdbId",
                table: "Games",
                column: "IgdbId",
                unique: true,
                filter: "[IgdbId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Franchises_FranchiseId",
                table: "Games",
                column: "FranchiseId",
                principalTable: "Franchises",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Franchises_FranchiseId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_IgdbId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "BackgroundImageUrl",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "CatalogProvider",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "FranchiseExternalId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "FranchiseName",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "IgdbId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "ReleasedAt",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "Games");

            migrationBuilder.AlterColumn<int>(
                name: "FranchiseId",
                table: "Games",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Franchises_FranchiseId",
                table: "Games",
                column: "FranchiseId",
                principalTable: "Franchises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
