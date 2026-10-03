using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tranqui.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAppeals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "appeals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    phone_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    kind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    contact_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_appeals", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hidden_numbers",
                columns: table => new
                {
                    phone_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    hidden_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hidden_numbers", x => x.phone_hash);
                });

            migrationBuilder.CreateTable(
                name: "sms_verification_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    phone_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    requested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sms_verification_requests", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_appeals_phone_hash_created_at",
                table: "appeals",
                columns: new[] { "phone_hash", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_sms_verification_requests_phone_hash_requested_at",
                table: "sms_verification_requests",
                columns: new[] { "phone_hash", "requested_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appeals");

            migrationBuilder.DropTable(
                name: "hidden_numbers");

            migrationBuilder.DropTable(
                name: "sms_verification_requests");
        }
    }
}
