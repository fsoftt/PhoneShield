using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tranqui.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReputation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contact_contributions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    phone_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    hash_key_version = table.Column<int>(type: "integer", nullable: false),
                    contributor_id = table.Column<byte[]>(type: "bytea", nullable: false),
                    name_ciphertext = table.Column<byte[]>(type: "bytea", nullable: true),
                    name_grouping_key = table.Column<byte[]>(type: "bytea", nullable: true),
                    contributed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contact_contributions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "spam_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    phone_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    hash_key_version = table.Column<int>(type: "integer", nullable: false),
                    contributor_id = table.Column<byte[]>(type: "bytea", nullable: false),
                    verdict = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    weight = table.Column<double>(type: "double precision", nullable: false),
                    label_ciphertext = table.Column<byte[]>(type: "bytea", nullable: true),
                    label_grouping_key = table.Column<byte[]>(type: "bytea", nullable: true),
                    reported_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_spam_reports", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_contact_contributions_phone_hash_contributor_id",
                table: "contact_contributions",
                columns: new[] { "phone_hash", "contributor_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_spam_reports_phone_hash_contributor_id",
                table: "spam_reports",
                columns: new[] { "phone_hash", "contributor_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contact_contributions");

            migrationBuilder.DropTable(
                name: "spam_reports");
        }
    }
}
