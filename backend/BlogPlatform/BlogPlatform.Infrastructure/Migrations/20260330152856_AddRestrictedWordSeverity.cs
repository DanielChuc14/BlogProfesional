using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRestrictedWordSeverity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "severity",
                table: "restricted_words",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "severity",
                table: "restricted_words");
        }
    }
}
