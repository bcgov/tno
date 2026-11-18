using System;
using TNO.DAL;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _107 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.DropForeignKey(
                name: "FK_content_license_license_id",
                table: "content");

            migrationBuilder.DropForeignKey(
                name: "FK_content_product_product_id",
                table: "content");

            migrationBuilder.DropForeignKey(
                name: "FK_content_series_series_id",
                table: "content");

            migrationBuilder.DropForeignKey(
                name: "FK_content_source_source_id",
                table: "content");

            migrationBuilder.DropIndex(
                name: "IX_topic_score_rule_source_id",
                table: "topic_score_rule");

            migrationBuilder.DropIndex(
                name: "IX_name9",
                table: "series");

            migrationBuilder.RenameIndex(
                name: "IX_name10",
                table: "source",
                newName: "IX_name9");

            migrationBuilder.RenameIndex(
                name: "IX_name11",
                table: "tag",
                newName: "IX_name10");

            migrationBuilder.RenameIndex(
                name: "IX_name12",
                table: "tone_pool",
                newName: "IX_name11");

            migrationBuilder.RenameIndex(
                name: "IX_name13",
                table: "topic",
                newName: "IX_name12");

            migrationBuilder.AlterColumn<long>(
                name: "version",
                table: "topic_score_rule",
                type: "bigint",
                nullable: false,
                defaultValueSql: "0",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_on",
                table: "topic_score_rule",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "updated_by",
                table: "topic_score_rule",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "section",
                table: "topic_score_rule",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_on",
                table: "topic_score_rule",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "topic_score_rule",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "series_id",
                table: "topic_score_rule",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "source_id",
                table: "series",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_source_id_series_id_section",
                table: "topic_score_rule",
                columns: new[] { "source_id", "series_id", "section" });

            migrationBuilder.CreateIndex(
                name: "IX_topic_score_rule_series_id",
                table: "topic_score_rule",
                column: "series_id");

            migrationBuilder.CreateIndex(
                name: "IX_series_source_id",
                table: "series",
                column: "source_id");

            migrationBuilder.AddForeignKey(
                name: "FK_content_license_license_id",
                table: "content",
                column: "license_id",
                principalTable: "license",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_content_product_product_id",
                table: "content",
                column: "product_id",
                principalTable: "product",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_content_series_series_id",
                table: "content",
                column: "series_id",
                principalTable: "series",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_content_source_source_id",
                table: "content",
                column: "source_id",
                principalTable: "source",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_series_source_source_id",
                table: "series",
                column: "source_id",
                principalTable: "source",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_topic_score_rule_series_series_id",
                table: "topic_score_rule",
                column: "series_id",
                principalTable: "series",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropForeignKey(
                name: "FK_content_license_license_id",
                table: "content");

            migrationBuilder.DropForeignKey(
                name: "FK_content_product_product_id",
                table: "content");

            migrationBuilder.DropForeignKey(
                name: "FK_content_series_series_id",
                table: "content");

            migrationBuilder.DropForeignKey(
                name: "FK_content_source_source_id",
                table: "content");

            migrationBuilder.DropForeignKey(
                name: "FK_series_source_source_id",
                table: "series");

            migrationBuilder.DropForeignKey(
                name: "FK_topic_score_rule_series_series_id",
                table: "topic_score_rule");

            migrationBuilder.DropIndex(
                name: "IX_source_id_series_id_section",
                table: "topic_score_rule");

            migrationBuilder.DropIndex(
                name: "IX_topic_score_rule_series_id",
                table: "topic_score_rule");

            migrationBuilder.DropIndex(
                name: "IX_series_source_id",
                table: "series");

            migrationBuilder.DropColumn(
                name: "series_id",
                table: "topic_score_rule");

            migrationBuilder.DropColumn(
                name: "source_id",
                table: "series");

            migrationBuilder.RenameIndex(
                name: "IX_name12",
                table: "topic",
                newName: "IX_name13");

            migrationBuilder.RenameIndex(
                name: "IX_name11",
                table: "tone_pool",
                newName: "IX_name12");

            migrationBuilder.RenameIndex(
                name: "IX_name10",
                table: "tag",
                newName: "IX_name11");

            migrationBuilder.RenameIndex(
                name: "IX_name9",
                table: "source",
                newName: "IX_name10");

            migrationBuilder.AlterColumn<long>(
                name: "version",
                table: "topic_score_rule",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValueSql: "0");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_on",
                table: "topic_score_rule",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "updated_by",
                table: "topic_score_rule",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "section",
                table: "topic_score_rule",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_on",
                table: "topic_score_rule",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "topic_score_rule",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250);

            migrationBuilder.CreateIndex(
                name: "IX_topic_score_rule_source_id",
                table: "topic_score_rule",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_name9",
                table: "series",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_content_license_license_id",
                table: "content",
                column: "license_id",
                principalTable: "license",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_content_product_product_id",
                table: "content",
                column: "product_id",
                principalTable: "product",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_content_series_series_id",
                table: "content",
                column: "series_id",
                principalTable: "series",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_content_source_source_id",
                table: "content",
                column: "source_id",
                principalTable: "source",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
            PostDown(migrationBuilder);
        }
    }
}
