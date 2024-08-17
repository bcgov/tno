using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _130 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.DropColumn(
                name: "is_subscribed",
                table: "user_product");

            migrationBuilder.DropColumn(
                name: "requested_is_subscribed_status",
                table: "user_product");

            migrationBuilder.DropColumn(
                name: "subscription_change_actioned",
                table: "user_product");

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "user_product",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropColumn(
                name: "status",
                table: "user_product");

            migrationBuilder.AddColumn<bool>(
                name: "is_subscribed",
                table: "user_product",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "requested_is_subscribed_status",
                table: "user_product",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "subscription_change_actioned",
                table: "user_product",
                type: "boolean",
                nullable: true);
            PostDown(migrationBuilder);
        }
    }
}
