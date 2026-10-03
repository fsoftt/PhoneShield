using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tranqui.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AppealQuotas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sms_verification_requests");

            migrationBuilder.CreateTable(
                name: "appeal_quota_usages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_kind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    subject_key = table.Column<byte[]>(type: "bytea", nullable: false),
                    action = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_appeal_quota_usages", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_appeal_quota_usages_subject_kind_subject_key_action_used_at",
                table: "appeal_quota_usages",
                columns: new[] { "subject_kind", "subject_key", "action", "used_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appeal_quota_usages");

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
                name: "ix_sms_verification_requests_phone_hash_requested_at",
                table: "sms_verification_requests",
                columns: new[] { "phone_hash", "requested_at" });
        }
    }
}
