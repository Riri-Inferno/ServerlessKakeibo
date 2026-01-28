using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerlessKakeibo.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRefreshTokenHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "RefreshTokenHash",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshTokenHash",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
