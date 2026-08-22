using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEventJoinCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JoinCode",
                table: "Events",
                type: "TEXT",
                maxLength: 6,
                nullable: true,
                collation: "NOCASE");

            migrationBuilder.Sql(
                """
                UPDATE Events
                SET JoinCode =
                    substr('ABCDEFGHJKLMNPQRSTUVWXYZ23456789', ((rowid / 33554432) % 32) + 1, 1) ||
                    substr('ABCDEFGHJKLMNPQRSTUVWXYZ23456789', ((rowid / 1048576) % 32) + 1, 1) ||
                    substr('ABCDEFGHJKLMNPQRSTUVWXYZ23456789', ((rowid / 32768) % 32) + 1, 1) ||
                    substr('ABCDEFGHJKLMNPQRSTUVWXYZ23456789', ((rowid / 1024) % 32) + 1, 1) ||
                    substr('ABCDEFGHJKLMNPQRSTUVWXYZ23456789', ((rowid / 32) % 32) + 1, 1) ||
                    substr('ABCDEFGHJKLMNPQRSTUVWXYZ23456789', (rowid % 32) + 1, 1);
                """);

            migrationBuilder.AlterColumn<string>(
                name: "JoinCode",
                table: "Events",
                type: "TEXT",
                maxLength: 6,
                nullable: false,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 6,
                oldNullable: true,
                oldCollation: "NOCASE");

            migrationBuilder.CreateIndex(
                name: "IX_Events_JoinCode",
                table: "Events",
                column: "JoinCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Events_JoinCode",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "JoinCode",
                table: "Events");
        }
    }
}
