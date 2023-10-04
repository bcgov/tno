using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1071 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.AddColumn<long>(
                name: "content_id",
                table: "work_order",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_work_order_content_id",
                table: "work_order",
                column: "content_id");

            migrationBuilder.AddForeignKey(
                name: "FK_work_order_content_content_id",
                table: "work_order",
                column: "content_id",
                principalTable: "content",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropForeignKey(
                name: "FK_work_order_content_content_id",
                table: "work_order");

            migrationBuilder.DropIndex(
                name: "IX_work_order_content_id",
                table: "work_order");

            migrationBuilder.DropColumn(
                name: "content_id",
                table: "work_order");
            PostDown(migrationBuilder);
        }
    }
}
