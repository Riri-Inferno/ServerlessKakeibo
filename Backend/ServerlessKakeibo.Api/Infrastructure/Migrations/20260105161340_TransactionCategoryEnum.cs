using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerlessKakeibo.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TransactionCategoryEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "CategoryName",
                table: "Transactions");

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Category",
                table: "Transactions",
                column: "Category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_Category",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Transactions");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoryName",
                table: "Transactions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
