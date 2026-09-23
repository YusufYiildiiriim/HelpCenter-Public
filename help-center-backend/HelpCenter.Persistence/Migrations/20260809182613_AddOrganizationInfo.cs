using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrganizationInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TaxNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TaxOffice = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FooterText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByAccountId = table.Column<int>(type: "int", nullable: true),
                    LastModifiedByAccountId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationInfos", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "CreatedAt", "CreatedByAccountId", "GroupTitle", "Icon", "IsActive", "IsDeleted", "Label", "LastModifiedByAccountId", "Order", "ParentId", "ResourceKey", "Route", "UpdatedAt" },
                values: new object[] { 14, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Building2", true, false, "Kurum Bilgileri", null, 14, null, "OrganizationSettings", "/admin/settings/organization", null });

            migrationBuilder.InsertData(
                table: "OrganizationInfos",
                columns: new[] { "Id", "Address", "CreatedAt", "CreatedByAccountId", "Email", "FooterText", "IsActive", "IsDeleted", "LastModifiedByAccountId", "LogoUrl", "OrganizationName", "Phone", "TaxNumber", "TaxOffice", "UpdatedAt", "Website" },
                values: new object[] { 1, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, true, false, null, null, "Help Center", null, null, null, null, null });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "AllowedFieldsJson", "CreatedAt", "CreatedByAccountId", "IsActive", "IsDeleted", "LastModifiedByAccountId", "ResourceKey", "RoleId", "UpdatedAt" },
                values: new object[] { 1001, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, "OrganizationSettings", 1, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationInfos");

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 19);
        }
    }
}
