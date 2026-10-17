using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations;

public partial class GuidIdentityCutover : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(GuidIdentityCutoverSql.Up);
        migrationBuilder.Sql(GuidIdentityCutoverSql.SpDashboardStats);
        migrationBuilder.Sql(GuidIdentityCutoverSql.SpDailyTrend);
        migrationBuilder.Sql(GuidIdentityCutoverSql.SpAvgResolutionMinutes);
        migrationBuilder.Sql(GuidIdentityCutoverSql.SpAgentPerformance);
        migrationBuilder.Sql(GuidIdentityCutoverSql.SpReopenStats);
        migrationBuilder.Sql(GuidIdentityCutoverSql.SpCompletedRequests);
        migrationBuilder.Sql(GuidIdentityCutoverSql.SpActiveRequests);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.Sql(GuidIdentityCutoverSql.Down);
}
