using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "CreatedAt", "CreatedByAccountId", "GroupTitle", "Icon", "IsActive", "IsDeleted", "Label", "LastModifiedByAccountId", "Order", "ParentId", "ResourceKey", "Route", "UpdatedAt" },
                values: new object[] { 15, new DateTime(2026, 10, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "ClipboardList", true, false, "Denetim Kayıtları", null, 15, null, "AuditLogs", "/admin/settings/audit-logs", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 15);
        }
    }
}
