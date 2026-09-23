using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpCenter.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveConversationParticipantLastReadAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastReadAt",
                table: "ConversationParticipants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastReadAt",
                table: "ConversationParticipants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ConversationParticipants",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastReadAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "ConversationParticipants",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastReadAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "ConversationParticipants",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastReadAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "ConversationParticipants",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastReadAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "ConversationParticipants",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastReadAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "ConversationParticipants",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastReadAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "ConversationParticipants",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastReadAt",
                value: null);
        }
    }
}
