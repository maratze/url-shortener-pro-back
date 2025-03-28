using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortenerPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHasPasswordSetToUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasPasswordSet",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasPasswordSet",
                table: "Users");
        }
    }
}
