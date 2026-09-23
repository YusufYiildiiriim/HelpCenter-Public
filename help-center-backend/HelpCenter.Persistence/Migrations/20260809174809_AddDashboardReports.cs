using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Reset (idempotent)
            foreach (var sp in new[] {
                "Sp_StatusDistribution", "Sp_ModuleDistribution", "Sp_CompanyTopN",
                "Sp_DailyTrend", "Sp_AvgResolutionMinutes",
                "Sp_AgentPerformance", "Sp_ReopenStats"
            })
            {
                migrationBuilder.Sql($"IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='{sp}') DROP PROCEDURE {sp};");
            }

            migrationBuilder.Sql(@"
            CREATE PROCEDURE Sp_StatusDistribution
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT s.Id AS StatusId, s.Name AS Name, COUNT(cr.Id) AS Count
                FROM CustomerRequestStatuses s
                LEFT JOIN CustomerRequests cr ON cr.StatusId = s.Id AND cr.IsDeleted = 0
                WHERE s.IsDeleted = 0
                GROUP BY s.Id, s.Name
                ORDER BY Count DESC;
            END;
            ");

            migrationBuilder.Sql(@"
            CREATE PROCEDURE Sp_ModuleDistribution
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT m.Id AS ModuleId, m.Name AS Name, COUNT(cr.Id) AS Count
                FROM Modules m
                LEFT JOIN CustomerRequests cr ON cr.ModuleId = m.Id AND cr.IsDeleted = 0
                WHERE m.IsDeleted = 0
                GROUP BY m.Id, m.Name
                ORDER BY Count DESC;
            END;
            ");

            migrationBuilder.Sql(@"
            CREATE PROCEDURE Sp_CompanyTopN
                @Top INT = 10
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT TOP (@Top) co.Id AS CompanyId, co.Name AS Name, COUNT(cr.Id) AS Count
                FROM Companies co
                LEFT JOIN Customers cu ON cu.CompanyId = co.Id AND cu.IsDeleted = 0
                LEFT JOIN CustomerRequests cr ON cr.CustomerId = cu.Id AND cr.IsDeleted = 0
                WHERE co.IsDeleted = 0
                GROUP BY co.Id, co.Name
                ORDER BY COUNT(cr.Id) DESC, co.Name ASC;
            END;
            ");

            migrationBuilder.Sql(@"
            CREATE PROCEDURE Sp_DailyTrend
                @Days INT = 30
            AS
            BEGIN
                SET NOCOUNT ON;
                DECLARE @start DATE = CAST(DATEADD(DAY, -(@Days - 1), GETDATE()) AS DATE);

                ;WITH DateSeq AS (
                    SELECT @start AS D
                    UNION ALL
                    SELECT DATEADD(DAY, 1, D) FROM DateSeq WHERE D < CAST(GETDATE() AS DATE)
                )
                SELECT
                    ds.D AS Date,
                    ISNULL(SUM(CASE WHEN CAST(cr.CreatedAt AS DATE) = ds.D THEN 1 ELSE 0 END), 0) AS Opened,
                    ISNULL(SUM(CASE WHEN cr.StatusId = 3 AND CAST(cr.UpdatedAt AS DATE) = ds.D THEN 1 ELSE 0 END), 0) AS Closed
                FROM DateSeq ds
                LEFT JOIN CustomerRequests cr
                    ON cr.IsDeleted = 0
                    AND (CAST(cr.CreatedAt AS DATE) = ds.D OR CAST(cr.UpdatedAt AS DATE) = ds.D)
                GROUP BY ds.D
                ORDER BY ds.D
                OPTION (MAXRECURSION 366);
            END;
            ");

            migrationBuilder.Sql(@"
            CREATE PROCEDURE Sp_AvgResolutionMinutes
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT ISNULL(AVG(CAST(DATEDIFF(MINUTE, CreatedAt, UpdatedAt) AS BIGINT)), 0) AS AvgMinutes
                FROM CustomerRequests
                WHERE IsDeleted = 0 AND StatusId = 3 AND UpdatedAt IS NOT NULL;
            END;
            ");

            migrationBuilder.Sql(@"
            CREATE PROCEDURE Sp_AgentPerformance
                @Top INT = 20
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT TOP (@Top)
                    u.Id AS UserId,
                    (a.FirstName + ' ' + a.LastName) AS FullName,
                    COUNT(cr.Id) AS Assigned,
                    SUM(CASE WHEN cr.StatusId = 3 THEN 1 ELSE 0 END) AS Completed,
                    ISNULL(AVG(CASE WHEN cr.StatusId = 3
                                    THEN CAST(DATEDIFF(MINUTE, cr.CreatedAt, cr.UpdatedAt) AS BIGINT) END), 0) AS AvgResolutionMinutes
                FROM Users u
                INNER JOIN Accounts a ON a.Id = u.AccountId
                LEFT JOIN CustomerRequests cr ON cr.AssignedUserId = u.Id AND cr.IsDeleted = 0
                WHERE u.IsDeleted = 0 AND u.IsActive = 1
                GROUP BY u.Id, a.FirstName, a.LastName
                HAVING COUNT(cr.Id) > 0
                ORDER BY Assigned DESC;
            END;
            ");

            migrationBuilder.Sql(@"
            CREATE PROCEDURE Sp_ReopenStats
            AS
            BEGIN
                SET NOCOUNT ON;
                DECLARE @totalCompleted INT = (SELECT COUNT(*) FROM CustomerRequests WHERE IsDeleted = 0 AND StatusId = 3);
                DECLARE @reopened INT = (
                    SELECT COUNT(DISTINCT rh.RequestId)
                    FROM RequestHistories rh
                    WHERE rh.Action = 2 AND rh.OldValue = '3' AND rh.IsDeleted = 0
                );

                SELECT
                    @totalCompleted AS TotalCompleted,
                    @reopened AS Reopened,
                    CASE WHEN @totalCompleted = 0 THEN 0
                         ELSE CAST((@reopened * 100.0 / @totalCompleted) AS INT) END AS RatePercent;
            END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var sp in new[] {
                "Sp_StatusDistribution", "Sp_ModuleDistribution", "Sp_CompanyTopN",
                "Sp_DailyTrend", "Sp_AvgResolutionMinutes",
                "Sp_AgentPerformance", "Sp_ReopenStats"
            })
            {
                migrationBuilder.Sql($"IF EXISTS (SELECT * FROM sys.objects WHERE type='P' AND name='{sp}') DROP PROCEDURE {sp};");
            }
        }
    }
}
