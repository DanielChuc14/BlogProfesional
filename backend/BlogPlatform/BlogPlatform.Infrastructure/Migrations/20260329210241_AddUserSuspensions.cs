using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSuspensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "suspended_until",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "user_suspensions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    suspended_by_admin_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    lifted_by_admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lifted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_suspensions", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_suspensions_users_lifted_by_admin_id",
                        column: x => x.lifted_by_admin_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_user_suspensions_users_suspended_by_admin_id",
                        column: x => x.suspended_by_admin_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_suspensions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_suspensions_lifted_by_admin_id",
                table: "user_suspensions",
                column: "lifted_by_admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_suspensions_suspended_by_admin_id",
                table: "user_suspensions",
                column: "suspended_by_admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_suspensions_user_id_is_active",
                table: "user_suspensions",
                columns: new[] { "user_id", "is_active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_suspensions");

            migrationBuilder.DropColumn(
                name: "suspended_until",
                table: "AspNetUsers");
        }
    }
}
