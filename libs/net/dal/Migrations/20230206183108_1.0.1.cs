using System.Text.Json;
using TNO.DAL;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _101 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.DropForeignKey(
                name: "FK_work_order_content_content_id",
                table: "work_order");

            migrationBuilder.DropIndex(
                name: "IX_work_order_content_id",
                table: "work_order");

            migrationBuilder.DropColumn(
                name: "content_id",
                table: "work_order");

            migrationBuilder.AddColumn<JsonDocument>(
                name: "configuration",
                table: "work_order",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb");

            migrationBuilder.AlterColumn<JsonDocument>(
                name: "configuration",
                table: "source",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb",
                oldClrType: typeof(string),
                oldType: "json");

            migrationBuilder.AlterColumn<JsonDocument>(
                name: "configuration",
                table: "ingest",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb",
                oldClrType: typeof(string),
                oldType: "json");

            migrationBuilder.AlterColumn<JsonDocument>(
                name: "configuration",
                table: "connection",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb",
                oldClrType: typeof(string),
                oldType: "json");
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropColumn(
                name: "configuration",
                table: "work_order");

            migrationBuilder.AddColumn<long>(
                name: "content_id",
                table: "work_order",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "configuration",
                table: "source",
                type: "json",
                nullable: false,
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb",
                oldDefaultValueSql: "'{}'::jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "configuration",
                table: "ingest",
                type: "json",
                nullable: false,
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb",
                oldDefaultValueSql: "'{}'::jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "configuration",
                table: "connection",
                type: "json",
                nullable: false,
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb",
                oldDefaultValueSql: "'{}'::jsonb");

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
                onDelete: ReferentialAction.Restrict);
            PostDown(migrationBuilder);
        }
    }
}
