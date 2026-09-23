using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuthorizationModelPhase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanManageFAQ",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CanManageRequests",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CanManageSettings",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CanManageUsers",
                table: "Roles");

            migrationBuilder.RenameColumn(
                name: "ModuleKey",
                table: "RolePermissions",
                newName: "ResourceKey");

            migrationBuilder.AddColumn<bool>(
                name: "CanApprove",
                table: "RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanAssign",
                table: "RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanChangeStatus",
                table: "RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanExport",
                table: "RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanPrint",
                table: "RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanReject",
                table: "RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanExport", "CanPrint", "CanReject" },
                values: new object[] { true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanExport", "CanPrint", "CanReject" },
                values: new object[] { true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanExport", "CanPrint", "CanReject" },
                values: new object[] { true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanExport", "CanPrint", "CanReject" },
                values: new object[] { true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanExport", "CanPrint", "CanReject" },
                values: new object[] { true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanExport", "CanPrint", "CanReject" },
                values: new object[] { true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanExport", "CanPrint", "CanReject" },
                values: new object[] { true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanExport", "CanPrint", "CanReject" },
                values: new object[] { true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanExport", "CanPrint", "CanReject" },
                values: new object[] { true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanExport", "CanPrint", "CanReject" },
                values: new object[] { true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanExport", "CanPrint", "CanReject" },
                values: new object[] { true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanExport", "CanPrint", "CanReject" },
                values: new object[] { true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanExport", "CanPrint", "CanReject" },
                values: new object[] { true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanExport", "CanPrint", "CanReject" },
                values: new object[] { true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanExport", "CanPrint", "CanReject" },
                values: new object[] { false, false, false, false, false, false });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanExport", "CanPrint", "CanReject" },
                values: new object[] { false, false, false, false, false, false });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanExport", "CanPrint", "CanReject", "DataScopeLevel" },
                values: new object[] { false, false, false, false, false, false, 1 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanExport", "CanPrint", "CanReject" },
                values: new object[] { true, true, true, true, true, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanApprove",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CanAssign",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CanChangeStatus",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CanExport",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CanPrint",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CanReject",
                table: "RolePermissions");

            migrationBuilder.RenameColumn(
                name: "ResourceKey",
                table: "RolePermissions",
                newName: "ModuleKey");

            migrationBuilder.AddColumn<bool>(
                name: "CanManageFAQ",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageRequests",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageSettings",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageUsers",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 17,
                column: "DataScopeLevel",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CanManageFAQ", "CanManageRequests", "CanManageSettings", "CanManageUsers" },
                values: new object[] { true, true, true, true });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CanManageFAQ", "CanManageRequests", "CanManageSettings", "CanManageUsers" },
                values: new object[] { false, false, false, false });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CanManageFAQ", "CanManageRequests", "CanManageSettings", "CanManageUsers" },
                values: new object[] { false, false, false, false });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CanManageFAQ", "CanManageRequests", "CanManageSettings", "CanManageUsers" },
                values: new object[] { false, false, false, false });
        }
    }
}
