using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tranqui.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BackOffice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "hash_key_version",
                table: "appeals",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "resolved_at",
                table: "appeals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cleared_numbers",
                columns: table => new
                {
                    phone_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    cleared_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cleared_numbers", x => x.phone_hash);
                });

            migrationBuilder.CreateIndex(
                name: "ix_appeals_status_created_at",
                table: "appeals",
                columns: new[] { "status", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cleared_numbers");

            migrationBuilder.DropIndex(
                name: "ix_appeals_status_created_at",
                table: "appeals");

            migrationBuilder.DropColumn(
                name: "hash_key_version",
                table: "appeals");

            migrationBuilder.DropColumn(
                name: "resolved_at",
                table: "appeals");
        }
    }
}
