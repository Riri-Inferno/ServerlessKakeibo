using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerlessKakeibo.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptSpecificFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicableCategory",
                table: "TaxDetails",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixedAmount",
                table: "TaxDetails",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TaxType",
                table: "TaxDetails",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InvoiceRegistrationNumber",
                table: "ShopDetails",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisteredBusinessName",
                table: "ShopDetails",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicableCategory",
                table: "TaxDetails");

            migrationBuilder.DropColumn(
                name: "IsFixedAmount",
                table: "TaxDetails");

            migrationBuilder.DropColumn(
                name: "TaxType",
                table: "TaxDetails");

            migrationBuilder.DropColumn(
                name: "InvoiceRegistrationNumber",
                table: "ShopDetails");

            migrationBuilder.DropColumn(
                name: "RegisteredBusinessName",
                table: "ShopDetails");
        }
    }
}
