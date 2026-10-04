using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tranqui.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SpamSignals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "block_signals",
                columns: table => new
                {
                    phone_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    contributor_id = table.Column<byte[]>(type: "bytea", nullable: false),
                    weight = table.Column<double>(type: "double precision", nullable: false),
                    blocked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_block_signals", x => new { x.phone_hash, x.contributor_id });
                });

            migrationBuilder.CreateTable(
                name: "contributor_reputations",
                columns: table => new
                {
                    contributor_id = table.Column<byte[]>(type: "bytea", nullable: false),
                    multiplier = table.Column<double>(type: "double precision", nullable: false),
                    computed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contributor_reputations", x => x.contributor_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_block_signals_contributor_id",
                table: "block_signals",
                column: "contributor_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "block_signals");

            migrationBuilder.DropTable(
                name: "contributor_reputations");
        }
    }
}
