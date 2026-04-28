using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerlessKakeibo.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDescount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ItemType",
                table: "TransactionItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemType",
                table: "TransactionItems");
        }
    }
}
