using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixAgentExpertPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agent (RoleId=2): Requests - tam CRUD
            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CanCreate", "CanUpdate", "CanDelete" },
                values: new object[] { true, true, true });

            // Agent (RoleId=2): AssignedRequests - tam CRUD
            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CanCreate", "CanUpdate", "CanDelete" },
                values: new object[] { true, true, true });

            // Expert (RoleId=4): AssignedRequests - tam CRUD
            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CanCreate", "CanUpdate", "CanDelete" },
                values: new object[] { true, true, true });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CanCreate", "CanUpdate", "CanDelete" },
                values: new object[] { false, false, false });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CanCreate", "CanUpdate", "CanDelete" },
                values: new object[] { false, false, false });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CanCreate", "CanUpdate", "CanDelete" },
                values: new object[] { false, false, false });
        }
    }
}
