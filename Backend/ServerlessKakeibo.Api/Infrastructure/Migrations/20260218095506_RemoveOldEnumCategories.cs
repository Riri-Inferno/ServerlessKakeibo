using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerlessKakeibo.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOldEnumCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_Category",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_TransactionItems_Category",
                table: "TransactionItems");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "TransactionItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "TransactionItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Category",
                table: "Transactions",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionItems_Category",
                table: "TransactionItems",
                column: "Category");
        }
    }
}
