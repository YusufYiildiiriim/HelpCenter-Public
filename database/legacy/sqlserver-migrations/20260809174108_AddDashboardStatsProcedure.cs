using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardStatsProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sp_DashboardStats') DROP PROCEDURE Sp_DashboardStats;
            ");

            migrationBuilder.Sql(@"
            CREATE PROCEDURE Sp_DashboardStats
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT
                    (SELECT COUNT(*) FROM CustomerRequests WHERE IsDeleted = 0)                                AS TotalRequests,
                    (SELECT COUNT(*) FROM CustomerRequests WHERE StatusId IN (1, 2, 4) AND IsDeleted = 0)      AS ActiveRequests,
                    (SELECT COUNT(*) FROM CustomerRequests WHERE StatusId = 3 AND IsDeleted = 0)               AS CompletedRequests,
                    (SELECT COUNT(*) FROM CustomerRequestMessages WHERE IsDeleted = 0)                         AS TotalMessages,
                    (SELECT COUNT(*) FROM Customers WHERE IsDeleted = 0 AND IsActive = 1)                      AS TotalCustomers,
                    (SELECT COUNT(*) FROM Companies WHERE IsDeleted = 0)                                       AS TotalCompanies;
            END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sp_DashboardStats') DROP PROCEDURE Sp_DashboardStats;");
        }
    }
}
