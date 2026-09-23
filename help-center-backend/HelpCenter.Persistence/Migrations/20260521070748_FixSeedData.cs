using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$9bF3tUO21DrmzbGnpKIXKOUKwUG7J7JmVMPmDm5yHf1oREBn66Kse");

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "CanCreate", "CanDelete", "CanRead", "CanUpdate", "CreatedAt", "CreatedByAccountId", "IsActive", "IsDeleted", "LastModifiedByAccountId", "ModuleKey", "RoleId", "UpdatedAt" },
                values: new object[,]
                {
                    { 12, true, true, true, true, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, "AssignedRequests", 1, null },
                    { 13, true, true, true, true, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, "Customers", 1, null },
                    { 14, true, true, true, true, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, "CustomerRequests", 1, null },
                    { 15, false, false, true, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, "Requests", 2, null },
                    { 16, false, false, true, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, "AssignedRequests", 2, null }
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CanManageFAQ", "CanManageRequests", "Description" },
                values: new object[] { false, false, "Müşteri Temsilcisi - Yalnızca talep okuma" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CanManageFAQ", "CanManageRequests", "CanManageSettings", "CanManageUsers", "CreatedAt", "CreatedByAccountId", "Description", "IsActive", "IsDeleted", "LastModifiedByAccountId", "Name", "UpdatedAt" },
                values: new object[] { 4, false, false, false, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Yetkili Kişi - Modül uzmanı, yalnızca atanan talepleri okur", true, false, null, "Expert", null });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "CanCreate", "CanDelete", "CanRead", "CanUpdate", "CreatedAt", "CreatedByAccountId", "IsActive", "IsDeleted", "LastModifiedByAccountId", "ModuleKey", "RoleId", "UpdatedAt" },
                values: new object[] { 17, false, false, true, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, null, "AssignedRequests", 4, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "admin");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CanManageFAQ", "CanManageRequests", "Description" },
                values: new object[] { true, true, "Müşteri Temsilcisi - Talep yönetimi" });
        }
    }
}
