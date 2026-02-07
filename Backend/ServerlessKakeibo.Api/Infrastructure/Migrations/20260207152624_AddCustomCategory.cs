using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerlessKakeibo.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserTransactionCategoryId",
                table: "Transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserIncomeItemCategoryId",
                table: "TransactionItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserItemCategoryId",
                table: "TransactionItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IncomeItemCategoryMasters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ColorCode = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsSystemDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeItemCategoryMasters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemCategoryMasters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ColorCode = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsSystemDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCategoryMasters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionCategoryMasters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ColorCode = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsIncome = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystemDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionCategoryMasters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserIncomeItemCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserSettingsId = table.Column<Guid>(type: "uuid", nullable: false),
                    MasterCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ColorCode = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsCustom = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserIncomeItemCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserIncomeItemCategories_IncomeItemCategoryMasters_MasterCa~",
                        column: x => x.MasterCategoryId,
                        principalTable: "IncomeItemCategoryMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserIncomeItemCategories_UserSettings_UserSettingsId",
                        column: x => x.UserSettingsId,
                        principalTable: "UserSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserItemCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserSettingsId = table.Column<Guid>(type: "uuid", nullable: false),
                    MasterCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ColorCode = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsCustom = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserItemCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserItemCategories_ItemCategoryMasters_MasterCategoryId",
                        column: x => x.MasterCategoryId,
                        principalTable: "ItemCategoryMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserItemCategories_UserSettings_UserSettingsId",
                        column: x => x.UserSettingsId,
                        principalTable: "UserSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTransactionCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserSettingsId = table.Column<Guid>(type: "uuid", nullable: false),
                    MasterCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ColorCode = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsIncome = table.Column<bool>(type: "boolean", nullable: false),
                    IsCustom = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTransactionCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTransactionCategories_TransactionCategoryMasters_Master~",
                        column: x => x.MasterCategoryId,
                        principalTable: "TransactionCategoryMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserTransactionCategories_UserSettings_UserSettingsId",
                        column: x => x.UserSettingsId,
                        principalTable: "UserSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserTransactionCategoryId",
                table: "Transactions",
                column: "UserTransactionCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionItems_UserIncomeItemCategoryId",
                table: "TransactionItems",
                column: "UserIncomeItemCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionItems_UserItemCategoryId",
                table: "TransactionItems",
                column: "UserItemCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeItemCategoryMasters_Code",
                table: "IncomeItemCategoryMasters",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomeItemCategoryMasters_DisplayOrder",
                table: "IncomeItemCategoryMasters",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategoryMasters_Code",
                table: "ItemCategoryMasters",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategoryMasters_DisplayOrder",
                table: "ItemCategoryMasters",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCategoryMasters_Code",
                table: "TransactionCategoryMasters",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCategoryMasters_DisplayOrder",
                table: "TransactionCategoryMasters",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_UserIncomeItemCategories_IsHidden",
                table: "UserIncomeItemCategories",
                column: "IsHidden");

            migrationBuilder.CreateIndex(
                name: "IX_UserIncomeItemCategories_MasterCategoryId",
                table: "UserIncomeItemCategories",
                column: "MasterCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserIncomeItemCategories_UserSettingsId",
                table: "UserIncomeItemCategories",
                column: "UserSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserIncomeItemCategories_UserSettingsId_DisplayOrder",
                table: "UserIncomeItemCategories",
                columns: new[] { "UserSettingsId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_UserItemCategories_IsHidden",
                table: "UserItemCategories",
                column: "IsHidden");

            migrationBuilder.CreateIndex(
                name: "IX_UserItemCategories_MasterCategoryId",
                table: "UserItemCategories",
                column: "MasterCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserItemCategories_UserSettingsId",
                table: "UserItemCategories",
                column: "UserSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserItemCategories_UserSettingsId_DisplayOrder",
                table: "UserItemCategories",
                columns: new[] { "UserSettingsId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_UserTransactionCategories_IsHidden",
                table: "UserTransactionCategories",
                column: "IsHidden");

            migrationBuilder.CreateIndex(
                name: "IX_UserTransactionCategories_MasterCategoryId",
                table: "UserTransactionCategories",
                column: "MasterCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTransactionCategories_UserSettingsId",
                table: "UserTransactionCategories",
                column: "UserSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTransactionCategories_UserSettingsId_DisplayOrder",
                table: "UserTransactionCategories",
                columns: new[] { "UserSettingsId", "DisplayOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionItems_UserIncomeItemCategories_UserIncomeItemCat~",
                table: "TransactionItems",
                column: "UserIncomeItemCategoryId",
                principalTable: "UserIncomeItemCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionItems_UserItemCategories_UserItemCategoryId",
                table: "TransactionItems",
                column: "UserItemCategoryId",
                principalTable: "UserItemCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_UserTransactionCategories_UserTransactionCateg~",
                table: "Transactions",
                column: "UserTransactionCategoryId",
                principalTable: "UserTransactionCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionItems_UserIncomeItemCategories_UserIncomeItemCat~",
                table: "TransactionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionItems_UserItemCategories_UserItemCategoryId",
                table: "TransactionItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_UserTransactionCategories_UserTransactionCateg~",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "UserIncomeItemCategories");

            migrationBuilder.DropTable(
                name: "UserItemCategories");

            migrationBuilder.DropTable(
                name: "UserTransactionCategories");

            migrationBuilder.DropTable(
                name: "IncomeItemCategoryMasters");

            migrationBuilder.DropTable(
                name: "ItemCategoryMasters");

            migrationBuilder.DropTable(
                name: "TransactionCategoryMasters");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserTransactionCategoryId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_TransactionItems_UserIncomeItemCategoryId",
                table: "TransactionItems");

            migrationBuilder.DropIndex(
                name: "IX_TransactionItems_UserItemCategoryId",
                table: "TransactionItems");

            migrationBuilder.DropColumn(
                name: "UserTransactionCategoryId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UserIncomeItemCategoryId",
                table: "TransactionItems");

            migrationBuilder.DropColumn(
                name: "UserItemCategoryId",
                table: "TransactionItems");
        }
    }
}
