using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MigrateUserProjectsToRoleScopes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // For each (UserId, ProjectId) pair, add a
            // RoleScope(ResourceTypeKey='Project', ResourceId=projectId) to the user's first active role.
            // Existing records are skipped. No-op if there is no UserProject data.
            migrationBuilder.Sql(@"
                INSERT INTO RoleScopes (RoleId, ResourceTypeKey, ResourceId, CreatedAt, IsDeleted, IsActive)
                SELECT ur.RoleId, 'Project', up.ProjectId, GETDATE(), 0, 1
                FROM UserProjects up
                INNER JOIN UserRoles ur ON ur.UserId = up.UserId AND ur.IsDeleted = 0 AND ur.IsActive = 1
                WHERE up.IsDeleted = 0 AND up.IsActive = 1
                  AND ur.RoleId = (
                      SELECT TOP 1 ur2.RoleId
                      FROM UserRoles ur2
                      WHERE ur2.UserId = up.UserId AND ur2.IsDeleted = 0 AND ur2.IsActive = 1
                      ORDER BY ur2.Id
                  )
                  AND NOT EXISTS (
                      SELECT 1 FROM RoleScopes rs
                      WHERE rs.RoleId = ur.RoleId
                        AND rs.ResourceTypeKey = 'Project'
                        AND rs.ResourceId = up.ProjectId
                        AND rs.IsDeleted = 0
                  );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Migrated records (only those produced by this migration) cannot be reverted
            // because RoleScopes are managed on a per-role basis from that point on. No-op.
        }
    }
}
