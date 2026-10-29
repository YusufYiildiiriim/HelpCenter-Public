using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAppLogsAndHardenAuditTrail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 7);

            // Older deployments can contain manually granted Logs permissions/menu rows in
            // addition to the seeded records above. The resource no longer exists, so remove
            // every obsolete record rather than leaving unreachable authorization state behind.
            migrationBuilder.Sql("DELETE FROM [MenuItems] WHERE [ResourceKey] = N'Logs' OR [Route] = N'/admin/logs';");
            migrationBuilder.Sql("DELETE FROM [RolePermissions] WHERE [ResourceKey] = N'Logs';");

            // Audit data is forensic evidence. Application code and ordinary DB clients may
            // append it, but cannot silently rewrite or remove existing evidence.
            migrationBuilder.Sql("""
                CREATE TRIGGER [dbo].[TR_AuditLogs_AppendOnly]
                ON [dbo].[AuditLogs]
                AFTER UPDATE, DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    THROW 51000, 'AuditLogs is append-only.', 1;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_AuditLogs_AppendOnly];");

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "CreatedAt", "CreatedByAccountId", "GroupTitle", "Icon", "IsActive", "IsDeleted", "Label", "LastModifiedByAccountId", "Order", "ParentId", "ResourceKey", "Route", "UpdatedAt" },
                values: new object[] { 9, new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Terminal", true, false, "Sistem Logları", null, 9, null, "Logs", "/admin/logs", null });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "AllowedFieldsJson", "CreatedAt", "CreatedByAccountId", "IsActive", "IsDeleted", "LastModifiedByAccountId", "ResourceKey", "RoleId", "UpdatedAt" },
                values: new object[] { 7, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, "Logs", 1, null });
        }
    }
}
