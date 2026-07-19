using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_stats",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    blog_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    view_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    unique_visitors = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    new_followers = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    likes_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    comments_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_daily_stats", x => x.id);
                    table.ForeignKey(
                        name: "fk_daily_stats_blog_profiles_blog_profile_id",
                        column: x => x.blog_profile_id,
                        principalTable: "blog_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_daily_stats_blog_profile_id_date",
                table: "daily_stats",
                columns: new[] { "blog_profile_id", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_daily_stats_date",
                table: "daily_stats",
                column: "date");

            // page_views: tabla particionada mensualmente por RANGE(created_at)
            // BIGSERIAL se declara manualmente porque EF no maneja tablas particionadas
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS page_views (
                    id          BIGSERIAL,
                    post_id     uuid        NOT NULL,
                    blog_profile_id uuid   NOT NULL,
                    visitor_hash character varying(64) NOT NULL,
                    ip_address  character varying(45),
                    user_agent  character varying(512),
                    referrer    character varying(512),
                    device_type character varying(20) NOT NULL DEFAULT 'Unknown',
                    created_at  timestamp with time zone NOT NULL DEFAULT NOW()
                ) PARTITION BY RANGE (created_at);
            ");

            // Partición inicial: mes actual (2026-03)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS page_views_y2026m03
                    PARTITION OF page_views
                    FOR VALUES FROM ('2026-03-01') TO ('2026-04-01');
            ");

            // Partición siguiente: 2026-04
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS page_views_y2026m04
                    PARTITION OF page_views
                    FOR VALUES FROM ('2026-04-01') TO ('2026-05-01');
            ");

            // Partición: 2026-05
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS page_views_y2026m05
                    PARTITION OF page_views
                    FOR VALUES FROM ('2026-05-01') TO ('2026-06-01');
            ");

            // Índices sobre la tabla particionada padre
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS ix_page_views_post_id ON page_views (post_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS ix_page_views_blog_profile_id ON page_views (blog_profile_id);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS ix_page_views_blog_profile_created ON page_views (blog_profile_id, created_at);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS page_views CASCADE;");

            migrationBuilder.DropTable(
                name: "daily_stats");
        }
    }
}
