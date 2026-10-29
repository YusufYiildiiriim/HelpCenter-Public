using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeDashboardDailyTrend : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // EXEC also permits procedure DDL inside EF's idempotent migration guards.
            migrationBuilder.Sql("""
                EXEC(N'
                CREATE OR ALTER PROCEDURE [dbo].[Sp_DailyTrend]
                    @Days INT = 30
                AS
                BEGIN
                    SET NOCOUNT ON;
                    DECLARE @today DATE = CAST(GETDATE() AS DATE);
                    DECLARE @start DATE = CAST(DATEADD(DAY, -(@Days - 1), GETDATE()) AS DATE);
                    -- Non-positive @Days historically produces one future date, not an empty range.
                    DECLARE @end DATE = DATEADD(DAY, 1, CASE WHEN @start > @today THEN @start ELSE @today END);

                    ;WITH DateSeq AS (
                        SELECT @start AS D
                        UNION ALL
                        SELECT DATEADD(DAY, 1, D) FROM DateSeq WHERE D < @today
                    ), Opened AS (
                        SELECT CAST(CreatedAt AS DATE) AS D, COUNT(*) AS Opened
                        FROM CustomerRequests
                        WHERE IsDeleted = 0 AND CreatedAt >= @start AND CreatedAt < @end
                        GROUP BY CAST(CreatedAt AS DATE)
                    ), Closed AS (
                        SELECT CAST(UpdatedAt AS DATE) AS D, COUNT(*) AS Closed
                        FROM CustomerRequests
                        WHERE IsDeleted = 0 AND StatusId = 3 AND UpdatedAt >= @start AND UpdatedAt < @end
                        GROUP BY CAST(UpdatedAt AS DATE)
                    )
                    SELECT
                        ds.D AS Date,
                        ISNULL(o.Opened, 0) AS Opened,
                        ISNULL(c.Closed, 0) AS Closed
                    FROM DateSeq ds
                    LEFT JOIN Opened o ON o.D = ds.D
                    LEFT JOIN Closed c ON c.D = ds.D
                    ORDER BY ds.D
                    OPTION (MAXRECURSION 366);
                END;
                ');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                EXEC(N'
                CREATE OR ALTER PROCEDURE [dbo].[Sp_DailyTrend]
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
                ');
                """);
        }
    }
}
