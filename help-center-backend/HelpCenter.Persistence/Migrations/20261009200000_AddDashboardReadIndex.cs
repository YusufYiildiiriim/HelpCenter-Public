using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardReadIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A SQL-only covering index for the dashboard read model. Exclude the
            // wide Description/Title columns; recent requests need only a few lookups.
            // Shared by the existing procedures and EF projections, without hints.
            migrationBuilder.Sql("""
                CREATE INDEX [IX_CustomerRequests_Dashboard_ActiveCreatedAt]
                ON [dbo].[CustomerRequests] ([CreatedAt] DESC, [Id] DESC)
                INCLUDE ([StatusId], [UpdatedAt], [AssignedUserId], [ModuleId], [CustomerId], [Priority])
                WHERE [IsDeleted] = 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX [IX_CustomerRequests_Dashboard_ActiveCreatedAt]
                ON [dbo].[CustomerRequests];
                """);
        }
    }
}
