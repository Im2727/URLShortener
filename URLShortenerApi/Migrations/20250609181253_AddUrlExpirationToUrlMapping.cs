using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URLShortenerApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUrlExpirationToUrlMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UrlMappings",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "UrlMappings",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UrlMappings");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "UrlMappings");
        }
    }
}
