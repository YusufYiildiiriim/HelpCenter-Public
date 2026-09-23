using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRolePermissionBoolColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "CanCreate",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CanDelete",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CanExport",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CanPrint",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CanRead",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CanReject",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CanUpdate",
                table: "RolePermissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "CanCreate",
                table: "RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanDelete",
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
                name: "CanRead",
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

            migrationBuilder.AddColumn<bool>(
                name: "CanUpdate",
                table: "RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanCreate", "CanDelete", "CanExport", "CanPrint", "CanRead", "CanReject", "CanUpdate" },
                values: new object[] { true, true, true, true, true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanCreate", "CanDelete", "CanExport", "CanPrint", "CanRead", "CanReject", "CanUpdate" },
                values: new object[] { true, true, true, true, true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanCreate", "CanDelete", "CanExport", "CanPrint", "CanRead", "CanReject", "CanUpdate" },
                values: new object[] { true, true, true, true, true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanCreate", "CanDelete", "CanExport", "CanPrint", "CanRead", "CanReject", "CanUpdate" },
                values: new object[] { true, true, true, true, true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanCreate", "CanDelete", "CanExport", "CanPrint", "CanRead", "CanReject", "CanUpdate" },
                values: new object[] { true, true, true, true, true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanCreate", "CanDelete", "CanExport", "CanPrint", "CanRead", "CanReject", "CanUpdate" },
                values: new object[] { true, true, true, true, true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanCreate", "CanDelete", "CanExport", "CanPrint", "CanRead", "CanReject", "CanUpdate" },
                values: new object[] { true, true, true, true, true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanCreate", "CanDelete", "CanExport", "CanPrint", "CanRead", "CanReject", "CanUpdate" },
                values: new object[] { true, true, true, true, true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanCreate", "CanDelete", "CanExport", "CanPrint", "CanRead", "CanReject", "CanUpdate" },
                values: new object[] { true, true, true, true, true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanCreate", "CanDelete", "CanExport", "CanPrint", "CanRead", "CanReject", "CanUpdate" },
                values: new object[] { true, true, true, true, true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanCreate", "CanDelete", "CanExport", "CanPrint", "CanRead", "CanReject", "CanUpdate" },
                values: new object[] { true, true, true, true, true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanCreate", "CanDelete", "CanExport", "CanPrint", "CanRead", "CanReject", "CanUpdate" },
                values: new object[] { true, true, true, true, true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanCreate", "CanDelete", "CanExport", "CanPrint", "CanRead", "CanReject", "CanUpdate" },
                values: new object[] { true, true, true, true, true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanCreate", "CanDelete", "CanExport", "CanPrint", "CanRead", "CanReject", "CanUpdate" },
                values: new object[] { true, true, true, true, true, true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanCreate", "CanDelete", "CanExport", "CanPrint", "CanRead", "CanReject", "CanUpdate" },
                values: new object[] { false, false, false, false, false, false, false, true, false, false });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanCreate", "CanDelete", "CanExport", "CanPrint", "CanRead", "CanReject", "CanUpdate" },
                values: new object[] { false, false, false, false, false, false, false, true, false, false });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanCreate", "CanDelete", "CanExport", "CanPrint", "CanRead", "CanReject", "CanUpdate" },
                values: new object[] { false, false, false, false, false, false, false, true, false, false });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "CanApprove", "CanAssign", "CanChangeStatus", "CanCreate", "CanDelete", "CanExport", "CanPrint", "CanRead", "CanReject", "CanUpdate" },
                values: new object[] { true, true, true, true, true, true, true, true, true, true });

            // The UPDATE target must be the alias `rp` — otherwise a Cartesian join
            // forms between the outer table and the alias, and if a single Actions record
            // matches, all RolePermissions rows are affected.
            migrationBuilder.Sql(@"
                UPDATE rp SET CanRead = 1 FROM RolePermissions rp WHERE EXISTS (SELECT 1 FROM RolePermissionActions rpa WHERE rpa.RolePermissionId = rp.Id AND rpa.Action = 'Read' AND rpa.IsDeleted = 0);
                UPDATE rp SET CanCreate = 1 FROM RolePermissions rp WHERE EXISTS (SELECT 1 FROM RolePermissionActions rpa WHERE rpa.RolePermissionId = rp.Id AND rpa.Action = 'Create' AND rpa.IsDeleted = 0);
                UPDATE rp SET CanUpdate = 1 FROM RolePermissions rp WHERE EXISTS (SELECT 1 FROM RolePermissionActions rpa WHERE rpa.RolePermissionId = rp.Id AND rpa.Action = 'Update' AND rpa.IsDeleted = 0);
                UPDATE rp SET CanDelete = 1 FROM RolePermissions rp WHERE EXISTS (SELECT 1 FROM RolePermissionActions rpa WHERE rpa.RolePermissionId = rp.Id AND rpa.Action = 'Delete' AND rpa.IsDeleted = 0);
                UPDATE rp SET CanExport = 1 FROM RolePermissions rp WHERE EXISTS (SELECT 1 FROM RolePermissionActions rpa WHERE rpa.RolePermissionId = rp.Id AND rpa.Action = 'Export' AND rpa.IsDeleted = 0);
                UPDATE rp SET CanPrint = 1 FROM RolePermissions rp WHERE EXISTS (SELECT 1 FROM RolePermissionActions rpa WHERE rpa.RolePermissionId = rp.Id AND rpa.Action = 'Print' AND rpa.IsDeleted = 0);
                UPDATE rp SET CanApprove = 1 FROM RolePermissions rp WHERE EXISTS (SELECT 1 FROM RolePermissionActions rpa WHERE rpa.RolePermissionId = rp.Id AND rpa.Action = 'Approve' AND rpa.IsDeleted = 0);
                UPDATE rp SET CanReject = 1 FROM RolePermissions rp WHERE EXISTS (SELECT 1 FROM RolePermissionActions rpa WHERE rpa.RolePermissionId = rp.Id AND rpa.Action = 'Reject' AND rpa.IsDeleted = 0);
                UPDATE rp SET CanAssign = 1 FROM RolePermissions rp WHERE EXISTS (SELECT 1 FROM RolePermissionActions rpa WHERE rpa.RolePermissionId = rp.Id AND rpa.Action = 'Assign' AND rpa.IsDeleted = 0);
                UPDATE rp SET CanChangeStatus = 1 FROM RolePermissions rp WHERE EXISTS (SELECT 1 FROM RolePermissionActions rpa WHERE rpa.RolePermissionId = rp.Id AND rpa.Action = 'ChangeStatus' AND rpa.IsDeleted = 0);
            ");
        }
    }
}
