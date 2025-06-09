using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URLShortenerApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRedirectCountToUrlMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RedirectCount",
                table: "UrlMappings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RedirectCount",
                table: "UrlMappings");
        }
    }
}
