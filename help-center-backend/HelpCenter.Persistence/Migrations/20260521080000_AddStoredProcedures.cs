using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sp_TotalRequest') DROP PROCEDURE Sp_TotalRequest;
            IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sp_TotalMessages') DROP PROCEDURE Sp_TotalMessages;
            IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sp_TotalCustomers') DROP PROCEDURE Sp_TotalCustomers;
            IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sp_CompletedRequests') DROP PROCEDURE Sp_CompletedRequests;
            IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sp_ActiveRequests') DROP PROCEDURE Sp_ActiveRequests;
            IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sp_TotalCompanies') DROP PROCEDURE Sp_TotalCompanies;
            ");

            migrationBuilder.Sql(@"
            CREATE PROCEDURE Sp_TotalRequest
            AS
            BEGIN
                SELECT COUNT(*) FROM CustomerRequests WHERE IsDeleted = 0;
            END;
            ");

            migrationBuilder.Sql(@"
            CREATE PROCEDURE Sp_TotalMessages
            AS
            BEGIN
                SELECT COUNT(*) FROM CustomerRequestMessages WHERE IsDeleted = 0;
            END;
            ");

            migrationBuilder.Sql(@"
            CREATE PROCEDURE Sp_TotalCustomers
            AS
            BEGIN
                SELECT COUNT(*) FROM Customers WHERE IsDeleted = 0 AND IsActive = 1;
            END;
            ");

            migrationBuilder.Sql(@"
            CREATE PROCEDURE Sp_CompletedRequests
            AS
            BEGIN
                SELECT COUNT(*) FROM CustomerRequests WHERE StatusId = 3 AND IsDeleted = 0;
            END;
            ");

            migrationBuilder.Sql(@"
            CREATE PROCEDURE Sp_ActiveRequests
            AS
            BEGIN
                SELECT COUNT(*) FROM CustomerRequests WHERE StatusId IN (1, 2, 4) AND IsDeleted = 0;
            END;
            ");

            migrationBuilder.Sql(@"
            CREATE PROCEDURE Sp_TotalCompanies
            AS
            BEGIN
                SELECT COUNT(*) FROM Companies WHERE IsDeleted = 0;
            END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sp_TotalRequest') DROP PROCEDURE Sp_TotalRequest;");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sp_TotalMessages') DROP PROCEDURE Sp_TotalMessages;");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sp_TotalCustomers') DROP PROCEDURE Sp_TotalCustomers;");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sp_CompletedRequests') DROP PROCEDURE Sp_CompletedRequests;");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sp_ActiveRequests') DROP PROCEDURE Sp_ActiveRequests;");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'Sp_TotalCompanies') DROP PROCEDURE Sp_TotalCompanies;");
        }
    }
}
