using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortenerPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthProviderToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthProvider",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthProvider",
                table: "Users");
        }
    }
}
