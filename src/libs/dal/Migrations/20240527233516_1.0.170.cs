using System;
using TNO.DAL;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _10170 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.DropPrimaryKey(
                name: "PK_user_product",
                table: "user_product");

            migrationBuilder.DropIndex(
                name: "IX_user_product_user_id",
                table: "user_product");

            migrationBuilder.AlterColumn<long>(
                name: "version",
                table: "user_product",
                type: "bigint",
                nullable: false,
                defaultValueSql: "0",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_on",
                table: "user_product",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "updated_by",
                table: "user_product",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_on",
                table: "user_product",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "user_product",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_product",
                table: "user_product",
                columns: new[] { "user_id", "product_id" });

            migrationBuilder.CreateIndex(
                name: "IX_user_product_product_id",
                table: "user_product",
                column: "product_id");
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropPrimaryKey(
                name: "PK_user_product",
                table: "user_product");

            migrationBuilder.DropIndex(
                name: "IX_user_product_product_id",
                table: "user_product");

            migrationBuilder.AlterColumn<long>(
                name: "version",
                table: "user_product",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValueSql: "0");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_on",
                table: "user_product",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "updated_by",
                table: "user_product",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_on",
                table: "user_product",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "user_product",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250);

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_product",
                table: "user_product",
                columns: new[] { "product_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "IX_user_product_user_id",
                table: "user_product",
                column: "user_id");
            PostDown(migrationBuilder);
        }
    }
}
