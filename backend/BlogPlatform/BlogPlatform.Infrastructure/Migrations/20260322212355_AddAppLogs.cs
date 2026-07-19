using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS app_logs (
                    id          BIGSERIAL    PRIMARY KEY,
                    log_date    TIMESTAMP    NOT NULL DEFAULT NOW(),
                    thread      VARCHAR(255),
                    log_level   VARCHAR(50)  NOT NULL,
                    logger      VARCHAR(500),
                    message     TEXT         NOT NULL,
                    exception   TEXT
                );
                CREATE INDEX IF NOT EXISTS ix_app_logs_log_date  ON app_logs (log_date DESC);
                CREATE INDEX IF NOT EXISTS ix_app_logs_log_level ON app_logs (log_level);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS app_logs;");
        }
    }
}
