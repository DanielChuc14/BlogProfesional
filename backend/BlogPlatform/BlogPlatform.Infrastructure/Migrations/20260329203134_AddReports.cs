using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reporter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_type = table.Column<int>(type: "integer", nullable: false),
                    target_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    reviewed_by_admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    admin_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reports", x => x.id);
                    table.ForeignKey(
                        name: "fk_reports_users_reporter_id",
                        column: x => x.reporter_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_reports_users_reviewed_by_admin_id",
                        column: x => x.reviewed_by_admin_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_reports_reporter_id_target_type_target_id",
                table: "reports",
                columns: new[] { "reporter_id", "target_type", "target_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reports_reviewed_by_admin_id",
                table: "reports",
                column: "reviewed_by_admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_reports_status",
                table: "reports",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reports");
        }
    }
}
