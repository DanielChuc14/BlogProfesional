using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "featured_order",
                table: "posts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "is_adult_content",
                table: "posts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_featured",
                table: "posts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "banner_url",
                table: "blog_profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "logo_url",
                table: "blog_profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tagline",
                table: "blog_profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "allow_adult_content",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "blog_lists",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    blog_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    slug = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    cover_image_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_lists", x => x.id);
                    table.ForeignKey(
                        name: "fk_blog_lists_blog_profiles_blog_profile_id",
                        column: x => x.blog_profile_id,
                        principalTable: "blog_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "blog_notices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    blog_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_notices", x => x.id);
                    table.ForeignKey(
                        name: "fk_blog_notices_blog_profiles_blog_profile_id",
                        column: x => x.blog_profile_id,
                        principalTable: "blog_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quick_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    blog_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quick_links", x => x.id);
                    table.ForeignKey(
                        name: "fk_quick_links_blog_profiles_blog_profile_id",
                        column: x => x.blog_profile_id,
                        principalTable: "blog_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "restricted_words",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    phrase = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_regex = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    added_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_restricted_words", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_word_filters",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    word = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_word_filters", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "blog_list_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    blog_list_id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_list_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_blog_list_items_blog_lists_blog_list_id",
                        column: x => x.blog_list_id,
                        principalTable: "blog_lists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_blog_list_items_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_blog_list_items_blog_list_id",
                table: "blog_list_items",
                column: "blog_list_id");

            migrationBuilder.CreateIndex(
                name: "ix_blog_list_items_blog_list_id_post_id",
                table: "blog_list_items",
                columns: new[] { "blog_list_id", "post_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_blog_list_items_post_id",
                table: "blog_list_items",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "ix_blog_lists_blog_profile_id",
                table: "blog_lists",
                column: "blog_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_blog_lists_blog_profile_id_slug",
                table: "blog_lists",
                columns: new[] { "blog_profile_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_blog_notices_blog_profile_id",
                table: "blog_notices",
                column: "blog_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_quick_links_blog_profile_id",
                table: "quick_links",
                column: "blog_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_restricted_words_phrase",
                table: "restricted_words",
                column: "phrase",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_word_filters_user_id",
                table: "user_word_filters",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_word_filters_user_id_word",
                table: "user_word_filters",
                columns: new[] { "user_id", "word" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blog_list_items");

            migrationBuilder.DropTable(
                name: "blog_notices");

            migrationBuilder.DropTable(
                name: "quick_links");

            migrationBuilder.DropTable(
                name: "restricted_words");

            migrationBuilder.DropTable(
                name: "user_word_filters");

            migrationBuilder.DropTable(
                name: "blog_lists");

            migrationBuilder.DropColumn(
                name: "featured_order",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "is_adult_content",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "is_featured",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "banner_url",
                table: "blog_profiles");

            migrationBuilder.DropColumn(
                name: "logo_url",
                table: "blog_profiles");

            migrationBuilder.DropColumn(
                name: "tagline",
                table: "blog_profiles");

            migrationBuilder.DropColumn(
                name: "allow_adult_content",
                table: "AspNetUsers");
        }
    }
}
