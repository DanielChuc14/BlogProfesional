using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMonetizationAndFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "blog_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    blog_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    price_cents = table.Column<int>(type: "integer", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    stripe_price_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_plans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stripe_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    blog_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stripe_account_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    details_submitted = table.Column<bool>(type: "boolean", nullable: false),
                    charges_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    payouts_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    country = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stripe_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    blog_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    blog_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    stripe_subscription_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    current_period_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    current_period_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    canceled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "fk_subscriptions_blog_plans_blog_plan_id",
                        column: x => x.blog_plan_id,
                        principalTable: "blog_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_cents = table.Column<int>(type: "integer", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    stripe_payment_intent_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    stripe_invoice_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_payments_subscriptions_subscription_id",
                        column: x => x.subscription_id,
                        principalTable: "subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_blog_plans_blog_profile_id",
                table: "blog_plans",
                column: "blog_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_stripe_payment_intent_id",
                table: "payments",
                column: "stripe_payment_intent_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_subscription_id",
                table: "payments",
                column: "subscription_id");

            migrationBuilder.CreateIndex(
                name: "ix_stripe_accounts_blog_profile_id",
                table: "stripe_accounts",
                column: "blog_profile_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stripe_accounts_stripe_account_id",
                table: "stripe_accounts",
                column: "stripe_account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subscriptions_blog_plan_id",
                table: "subscriptions",
                column: "blog_plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_subscriptions_blog_profile_id",
                table: "subscriptions",
                column: "blog_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_subscriptions_stripe_subscription_id",
                table: "subscriptions",
                column: "stripe_subscription_id");

            migrationBuilder.CreateIndex(
                name: "ix_subscriptions_user_id",
                table: "subscriptions",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "stripe_accounts");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropTable(
                name: "blog_plans");
        }
    }
}
