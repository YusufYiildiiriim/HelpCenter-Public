using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnforceAuditLogAppendOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE FUNCTION "RejectAuditLogMutation"()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $audit$
                BEGIN
                    RAISE EXCEPTION 'AuditLogs is append-only.' USING ERRCODE = '55000';
                END;
                $audit$;

                CREATE TRIGGER "TR_AuditLogs_AppendOnly"
                BEFORE UPDATE OR DELETE ON "AuditLogs"
                FOR EACH ROW EXECUTE FUNCTION "RejectAuditLogMutation"();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TR_AuditLogs_AppendOnly" ON "AuditLogs";
                DROP FUNCTION IF EXISTS "RejectAuditLogMutation"();
                """);
        }
    }
}
