using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace BlogPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "blog_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    about = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    is_monetization_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_profiles", x => x.id);
                    table.ForeignKey(
                        name: "fk_blog_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    post_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "blog_themes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    blog_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    config = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_themes", x => x.id);
                    table.ForeignKey(
                        name: "fk_blog_themes_blog_profiles_blog_profile_id",
                        column: x => x.blog_profile_id,
                        principalTable: "blog_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "posts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    blog_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "character varying(350)", maxLength: 350, nullable: false),
                    excerpt = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cover_image_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    read_time_minutes = table.Column<int>(type: "integer", nullable: false),
                    view_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    likes_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    comments_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_posts", x => x.id);
                    table.ForeignKey(
                        name: "fk_posts_blog_profiles_blog_profile_id",
                        column: x => x.blog_profile_id,
                        principalTable: "blog_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "social_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    blog_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    platform = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_social_links", x => x.id);
                    table.ForeignKey(
                        name: "fk_social_links_blog_profiles_blog_profile_id",
                        column: x => x.blog_profile_id,
                        principalTable: "blog_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_media",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    caption = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_media", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_media_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_seos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    meta_title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    meta_description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    og_image_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    canonical_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_seos", x => x.id);
                    table.ForeignKey(
                        name: "fk_post_seos_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_tags",
                columns: table => new
                {
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_tags", x => new { x.post_id, x.tag_id });
                    table.ForeignKey(
                        name: "fk_post_tags_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_post_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_blog_profiles_slug",
                table: "blog_profiles",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_blog_profiles_user_id",
                table: "blog_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_blog_themes_blog_profile_id",
                table: "blog_themes",
                column: "blog_profile_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_post_media_post_id",
                table: "post_media",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_post_seos_post_id",
                table: "post_seos",
                column: "post_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_post_tags_tag_id",
                table: "post_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_posts_blog_profile_id",
                table: "posts",
                column: "blog_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_posts_published_at",
                table: "posts",
                column: "published_at");

            migrationBuilder.CreateIndex(
                name: "ix_posts_search_vector",
                table: "posts",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "ix_posts_slug",
                table: "posts",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_posts_status",
                table: "posts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_social_links_blog_profile_id",
                table: "social_links",
                column: "blog_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_tags_name",
                table: "tags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tags_slug",
                table: "tags",
                column: "slug",
                unique: true);

            // trgm index on tags.name for autocomplete
            migrationBuilder.Sql("CREATE INDEX idx_tags_name_trgm ON tags USING GIN(name gin_trgm_ops);");

            // Trigger function to auto-update posts.search_vector
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION fn_update_post_search_vector()
                RETURNS TRIGGER AS $$
                BEGIN
                    NEW.search_vector := to_tsvector('english',
                        COALESCE(NEW.title, '') || ' ' ||
                        COALESCE(NEW.content, ''));
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER trg_posts_search_vector
                BEFORE INSERT OR UPDATE ON posts
                FOR EACH ROW EXECUTE FUNCTION fn_update_post_search_vector();
            ");

            // Triggers updated_at for new tables
            migrationBuilder.Sql(@"
                CREATE TRIGGER trg_blog_profiles_updated_at
                BEFORE UPDATE ON blog_profiles
                FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER trg_blog_themes_updated_at
                BEFORE UPDATE ON blog_themes
                FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER trg_social_links_updated_at
                BEFORE UPDATE ON social_links
                FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER trg_posts_updated_at
                BEFORE UPDATE ON posts
                FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER trg_post_seos_updated_at
                BEFORE UPDATE ON post_seos
                FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER trg_post_media_updated_at
                BEFORE UPDATE ON post_media
                FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER trg_tags_updated_at
                BEFORE UPDATE ON tags
                FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_tags_updated_at ON tags;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_post_media_updated_at ON post_media;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_post_seos_updated_at ON post_seos;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_posts_updated_at ON posts;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_posts_search_vector ON posts;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_social_links_updated_at ON social_links;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_blog_themes_updated_at ON blog_themes;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_blog_profiles_updated_at ON blog_profiles;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS fn_update_post_search_vector();");
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_tags_name_trgm;");

            migrationBuilder.DropTable(
                name: "blog_themes");

            migrationBuilder.DropTable(
                name: "post_media");

            migrationBuilder.DropTable(
                name: "post_seos");

            migrationBuilder.DropTable(
                name: "post_tags");

            migrationBuilder.DropTable(
                name: "social_links");

            migrationBuilder.DropTable(
                name: "posts");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "blog_profiles");
        }
    }
}
