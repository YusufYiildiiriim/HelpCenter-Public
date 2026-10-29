using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRolePermissionActions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RolePermissionActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RolePermissionId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(450)", nullable: false),
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
                    table.PrimaryKey("PK_RolePermissionActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissionActions_RolePermissions_RolePermissionId",
                        column: x => x.RolePermissionId,
                        principalTable: "RolePermissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissionActions_RolePermissionId_Action",
                table: "RolePermissionActions",
                columns: new[] { "RolePermissionId", "Action" },
                unique: true);

            // Data migration: convert the existing CanX bool columns into action rows.
            // This way no user loses permissions when the PermissionHandler reads from the new table.
            var standardActions = new (string Column, string Action)[]
            {
                ("CanRead",         "Read"),
                ("CanCreate",       "Create"),
                ("CanUpdate",       "Update"),
                ("CanDelete",       "Delete"),
                ("CanExport",       "Export"),
                ("CanPrint",        "Print"),
                ("CanApprove",      "Approve"),
                ("CanReject",       "Reject"),
                ("CanAssign",       "Assign"),
                ("CanChangeStatus", "ChangeStatus"),
            };

            foreach (var (column, action) in standardActions)
            {
                migrationBuilder.Sql($@"
INSERT INTO RolePermissionActions (RolePermissionId, Action, CreatedAt, IsDeleted, IsActive)
SELECT Id, '{action}', SYSUTCDATETIME(), 0, 1
FROM RolePermissions
WHERE {column} = 1 AND IsDeleted = 0;");
            }

            // Add the new contextual actions to the Admin role (RoleId=1) for semantically appropriate resources.
            var adminContextualGrants = new (string Resource, string Action)[]
            {
                ("Projects",  "ManageMembers"),
                ("Projects",  "ManageModules"),
                ("Modules",   "ManageExperts"),
                ("Companies", "ManageMembers"),
                ("Companies", "ManageProjects"),
            };

            foreach (var (resource, action) in adminContextualGrants)
            {
                migrationBuilder.Sql($@"
INSERT INTO RolePermissionActions (RolePermissionId, Action, CreatedAt, IsDeleted, IsActive)
SELECT rp.Id, '{action}', SYSUTCDATETIME(), 0, 1
FROM RolePermissions rp
WHERE rp.RoleId = 1 AND rp.ResourceKey = '{resource}' AND rp.IsDeleted = 0
  AND NOT EXISTS (
    SELECT 1 FROM RolePermissionActions a
    WHERE a.RolePermissionId = rp.Id AND a.Action = '{action}'
  );");
            }

            // LookupSelect: add as a baseline to every RolePermission row of the Admin role
            migrationBuilder.Sql(@"
INSERT INTO RolePermissionActions (RolePermissionId, Action, CreatedAt, IsDeleted, IsActive)
SELECT rp.Id, 'LookupSelect', SYSUTCDATETIME(), 0, 1
FROM RolePermissions rp
WHERE rp.RoleId = 1 AND rp.IsDeleted = 0
  AND NOT EXISTS (
    SELECT 1 FROM RolePermissionActions a
    WHERE a.RolePermissionId = rp.Id AND a.Action = 'LookupSelect'
  );");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolePermissionActions");
        }
    }
}
