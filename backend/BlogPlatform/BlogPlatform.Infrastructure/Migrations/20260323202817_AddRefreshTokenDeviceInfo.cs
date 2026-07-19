using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenDeviceInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clear existing plain-text tokens before shrinking the column to 64 chars (SHA-256 hex)
            migrationBuilder.Sql("DELETE FROM refresh_tokens;");

            migrationBuilder.AlterColumn<string>(
                name: "token",
                table: "refresh_tokens",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldMaxLength: 512);

            migrationBuilder.AddColumn<string>(
                name: "device_info",
                table: "refresh_tokens",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "country",
                table: "AspNetUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "git_hub_url",
                table: "AspNetUsers",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "linked_in_url",
                table: "AspNetUsers",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "profile_header_color",
                table: "AspNetUsers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "storage_limit_bytes",
                table: "AspNetUsers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "storage_used_bytes",
                table: "AspNetUsers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "twitter_handle",
                table: "AspNetUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "website",
                table: "AspNetUsers",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ip_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "newsletter_sends",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    blog_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    html_body = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    estimated_recipients = table.Column<int>(type: "integer", nullable: false),
                    actual_recipients = table.Column<int>(type: "integer", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_newsletter_sends", x => x.id);
                    table.ForeignKey(
                        name: "fk_newsletter_sends_blog_profiles_blog_profile_id",
                        column: x => x.blog_profile_id,
                        principalTable: "blog_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "platform_settings",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    value = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_platform_settings", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "slug_redirects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    old_slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_slug_redirects", x => x.id);
                    table.ForeignKey(
                        name: "fk_slug_redirects_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_actor_id",
                table: "audit_logs",
                column: "actor_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_created_at",
                table: "audit_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_newsletter_sends_blog_profile_id_created_at",
                table: "newsletter_sends",
                columns: new[] { "blog_profile_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_slug_redirects_old_slug",
                table: "slug_redirects",
                column: "old_slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_slug_redirects_post_id",
                table: "slug_redirects",
                column: "post_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "newsletter_sends");

            migrationBuilder.DropTable(
                name: "platform_settings");

            migrationBuilder.DropTable(
                name: "slug_redirects");

            migrationBuilder.DropColumn(
                name: "device_info",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "country",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "git_hub_url",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "linked_in_url",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "profile_header_color",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "storage_limit_bytes",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "storage_used_bytes",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "twitter_handle",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "website",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "token",
                table: "refresh_tokens",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);
        }
    }
}
