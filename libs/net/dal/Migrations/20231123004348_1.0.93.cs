using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1093 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.DropForeignKey(
                name: "FK_product_user_owner_id",
                table: "product");

            migrationBuilder.DropIndex(
                name: "IX_product_owner_id_name_target_product_id_product_type",
                table: "product");

            migrationBuilder.DropColumn(
                name: "owner_id",
                table: "product");

            migrationBuilder.AddColumn<bool>(
                name: "is_public",
                table: "product",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_product_name_target_product_id_product_type",
                table: "product",
                columns: new[] { "name", "target_product_id", "product_type" },
                unique: true);
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropIndex(
                name: "IX_product_name_target_product_id_product_type",
                table: "product");

            migrationBuilder.DropColumn(
                name: "is_public",
                table: "product");

            migrationBuilder.AddColumn<int>(
                name: "owner_id",
                table: "product",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_owner_id_name_target_product_id_product_type",
                table: "product",
                columns: new[] { "owner_id", "name", "target_product_id", "product_type" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_product_user_owner_id",
                table: "product",
                column: "owner_id",
                principalTable: "user",
                principalColumn: "id");
            PostDown(migrationBuilder);
        }
    }
}
