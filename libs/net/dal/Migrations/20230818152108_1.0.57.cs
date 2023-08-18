using System;
using TNO.DAL;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1057 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.DropIndex(
                name: "IX_avoverviewtemplatesection_is_enabled",
                table: "av_overview_template_section");

            migrationBuilder.DropPrimaryKey(
                name: "PK_av_overview_template",
                table: "av_overview_template");

            migrationBuilder.DropIndex(
                name: "IX_av_overview_template_name",
                table: "av_overview_template");

            migrationBuilder.DropIndex(
                name: "IX_avoverviewtemplate_is_enabled",
                table: "av_overview_template");

            migrationBuilder.DropIndex(
                name: "IX_avoverviewsectionitem_is_enabled",
                table: "av_overview_section_item");

            migrationBuilder.DropIndex(
                name: "IX_avoverviewsection_is_enabled",
                table: "av_overview_section");

            migrationBuilder.DropIndex(
                name: "IX_avoverviewinstance_is_enabled",
                table: "av_overview_instance");

            migrationBuilder.DropColumn(
                name: "description",
                table: "av_overview_template_section");

            migrationBuilder.DropColumn(
                name: "is_enabled",
                table: "av_overview_template_section");

            migrationBuilder.DropColumn(
                name: "description",
                table: "av_overview_template");

            migrationBuilder.DropColumn(
                name: "is_enabled",
                table: "av_overview_template");

            migrationBuilder.DropColumn(
                name: "name",
                table: "av_overview_template");

            migrationBuilder.DropColumn(
                name: "sort_order",
                table: "av_overview_template");

            migrationBuilder.DropColumn(
                name: "template",
                table: "av_overview_template");

            migrationBuilder.DropColumn(
                name: "description",
                table: "av_overview_section_item");

            migrationBuilder.DropColumn(
                name: "is_enabled",
                table: "av_overview_section_item");

            migrationBuilder.DropColumn(
                name: "name",
                table: "av_overview_section_item");

            migrationBuilder.DropColumn(
                name: "av_overview_template_id",
                table: "av_overview_section");

            migrationBuilder.DropColumn(
                name: "description",
                table: "av_overview_section");

            migrationBuilder.DropColumn(
                name: "is_enabled",
                table: "av_overview_section");

            migrationBuilder.DropColumn(
                name: "description",
                table: "av_overview_instance");

            migrationBuilder.DropColumn(
                name: "name",
                table: "av_overview_instance");

            migrationBuilder.DropColumn(
                name: "sort_order",
                table: "av_overview_instance");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "av_overview_template",
                newName: "report_template_id");

            migrationBuilder.RenameColumn(
                name: "is_enabled",
                table: "av_overview_instance",
                newName: "IsPublished");

            migrationBuilder.AddColumn<bool>(
                name: "is_subscribed",
                table: "user_report",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_subscribed",
                table: "user_notification",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "report_type",
                table: "report_template",
                type: "integer",
                nullable: false,
                defaultValueSql: "0");

            migrationBuilder.AlterColumn<string>(
                name: "start_time",
                table: "av_overview_template_section",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "sort_order",
                table: "av_overview_template_section",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "other_source",
                table: "av_overview_template_section",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "av_overview_template_section",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "anchors",
                table: "av_overview_template_section",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "report_template_id",
                table: "av_overview_template",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "time",
                table: "av_overview_section_item",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "summary",
                table: "av_overview_section_item",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<int>(
                name: "sort_order",
                table: "av_overview_section_item",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "start_time",
                table: "av_overview_section",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "sort_order",
                table: "av_overview_section",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "other_source",
                table: "av_overview_section",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "av_overview_section",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "anchors",
                table: "av_overview_section",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<DateTime>(
                name: "published_on",
                table: "av_overview_instance",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_av_overview_template",
                table: "av_overview_template",
                column: "template_type");

            migrationBuilder.CreateTable(
                name: "av_overview_template_section_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    av_overview_template_section_id = table.Column<int>(type: "integer", nullable: false),
                    item_type = table.Column<int>(type: "integer", nullable: false),
                    time = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_av_overview_template_section_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_av_overview_template_section_item_av_overview_template_sect~",
                        column: x => x.av_overview_template_section_id,
                        principalTable: "av_overview_template_section",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_av_overview",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    av_overview_template_type = table.Column<int>(type: "integer", nullable: false),
                    is_subscribed = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_av_overview", x => new { x.user_id, x.av_overview_template_type });
                    table.ForeignKey(
                        name: "FK_user_av_overview_av_overview_template_av_overview_template_~",
                        column: x => x.av_overview_template_type,
                        principalTable: "av_overview_template",
                        principalColumn: "template_type",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_av_overview_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_template_section_av_overview_template_id",
                table: "av_overview_template_section",
                column: "av_overview_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_template_section_series_id",
                table: "av_overview_template_section",
                column: "series_id");

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_template_section_source_id",
                table: "av_overview_template_section",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_template_report_template_id",
                table: "av_overview_template",
                column: "report_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_section_item_av_overview_section_id",
                table: "av_overview_section_item",
                column: "av_overview_section_id");

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_section_item_content_id",
                table: "av_overview_section_item",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_section_av_overview_instance_id",
                table: "av_overview_section",
                column: "av_overview_instance_id");

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_section_series_id",
                table: "av_overview_section",
                column: "series_id");

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_section_source_id",
                table: "av_overview_section",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_instance_published_on",
                table: "av_overview_instance",
                column: "published_on",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_instance_template_type_published_on",
                table: "av_overview_instance",
                columns: new[] { "template_type", "published_on" });

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_template_section_item_av_overview_template_sect~",
                table: "av_overview_template_section_item",
                column: "av_overview_template_section_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_av_overview_av_overview_template_type",
                table: "user_av_overview",
                column: "av_overview_template_type");

            migrationBuilder.AddForeignKey(
                name: "FK_av_overview_instance_av_overview_template_template_type",
                table: "av_overview_instance",
                column: "template_type",
                principalTable: "av_overview_template",
                principalColumn: "template_type",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_av_overview_section_av_overview_instance_av_overview_instan~",
                table: "av_overview_section",
                column: "av_overview_instance_id",
                principalTable: "av_overview_instance",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_av_overview_section_series_series_id",
                table: "av_overview_section",
                column: "series_id",
                principalTable: "series",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_av_overview_section_source_source_id",
                table: "av_overview_section",
                column: "source_id",
                principalTable: "source",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_av_overview_section_item_av_overview_section_av_overview_se~",
                table: "av_overview_section_item",
                column: "av_overview_section_id",
                principalTable: "av_overview_section",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_av_overview_section_item_content_content_id",
                table: "av_overview_section_item",
                column: "content_id",
                principalTable: "content",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_av_overview_template_report_template_report_template_id",
                table: "av_overview_template",
                column: "report_template_id",
                principalTable: "report_template",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_av_overview_template_section_av_overview_template_av_overvi~",
                table: "av_overview_template_section",
                column: "av_overview_template_id",
                principalTable: "av_overview_template",
                principalColumn: "template_type",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_av_overview_template_section_series_series_id",
                table: "av_overview_template_section",
                column: "series_id",
                principalTable: "series",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_av_overview_template_section_source_source_id",
                table: "av_overview_template_section",
                column: "source_id",
                principalTable: "source",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropForeignKey(
                name: "FK_av_overview_instance_av_overview_template_template_type",
                table: "av_overview_instance");

            migrationBuilder.DropForeignKey(
                name: "FK_av_overview_section_av_overview_instance_av_overview_instan~",
                table: "av_overview_section");

            migrationBuilder.DropForeignKey(
                name: "FK_av_overview_section_series_series_id",
                table: "av_overview_section");

            migrationBuilder.DropForeignKey(
                name: "FK_av_overview_section_source_source_id",
                table: "av_overview_section");

            migrationBuilder.DropForeignKey(
                name: "FK_av_overview_section_item_av_overview_section_av_overview_se~",
                table: "av_overview_section_item");

            migrationBuilder.DropForeignKey(
                name: "FK_av_overview_section_item_content_content_id",
                table: "av_overview_section_item");

            migrationBuilder.DropForeignKey(
                name: "FK_av_overview_template_report_template_report_template_id",
                table: "av_overview_template");

            migrationBuilder.DropForeignKey(
                name: "FK_av_overview_template_section_av_overview_template_av_overvi~",
                table: "av_overview_template_section");

            migrationBuilder.DropForeignKey(
                name: "FK_av_overview_template_section_series_series_id",
                table: "av_overview_template_section");

            migrationBuilder.DropForeignKey(
                name: "FK_av_overview_template_section_source_source_id",
                table: "av_overview_template_section");

            migrationBuilder.DropTable(
                name: "av_overview_template_section_item");

            migrationBuilder.DropTable(
                name: "user_av_overview");

            migrationBuilder.DropIndex(
                name: "IX_av_overview_template_section_av_overview_template_id",
                table: "av_overview_template_section");

            migrationBuilder.DropIndex(
                name: "IX_av_overview_template_section_series_id",
                table: "av_overview_template_section");

            migrationBuilder.DropIndex(
                name: "IX_av_overview_template_section_source_id",
                table: "av_overview_template_section");

            migrationBuilder.DropPrimaryKey(
                name: "PK_av_overview_template",
                table: "av_overview_template");

            migrationBuilder.DropIndex(
                name: "IX_av_overview_template_report_template_id",
                table: "av_overview_template");

            migrationBuilder.DropIndex(
                name: "IX_av_overview_section_item_av_overview_section_id",
                table: "av_overview_section_item");

            migrationBuilder.DropIndex(
                name: "IX_av_overview_section_item_content_id",
                table: "av_overview_section_item");

            migrationBuilder.DropIndex(
                name: "IX_av_overview_section_av_overview_instance_id",
                table: "av_overview_section");

            migrationBuilder.DropIndex(
                name: "IX_av_overview_section_series_id",
                table: "av_overview_section");

            migrationBuilder.DropIndex(
                name: "IX_av_overview_section_source_id",
                table: "av_overview_section");

            migrationBuilder.DropIndex(
                name: "IX_av_overview_instance_published_on",
                table: "av_overview_instance");

            migrationBuilder.DropIndex(
                name: "IX_av_overview_instance_template_type_published_on",
                table: "av_overview_instance");

            migrationBuilder.DropColumn(
                name: "is_subscribed",
                table: "user_report");

            migrationBuilder.DropColumn(
                name: "is_subscribed",
                table: "user_notification");

            migrationBuilder.DropColumn(
                name: "report_type",
                table: "report_template");

            migrationBuilder.RenameColumn(
                name: "report_template_id",
                table: "av_overview_template",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "IsPublished",
                table: "av_overview_instance",
                newName: "is_enabled");

            migrationBuilder.AlterColumn<string>(
                name: "start_time",
                table: "av_overview_template_section",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(8)",
                oldMaxLength: 8);

            migrationBuilder.AlterColumn<int>(
                name: "sort_order",
                table: "av_overview_template_section",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "other_source",
                table: "av_overview_template_section",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "av_overview_template_section",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "anchors",
                table: "av_overview_template_section",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "av_overview_template_section",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValueSql: "''");

            migrationBuilder.AddColumn<bool>(
                name: "is_enabled",
                table: "av_overview_template_section",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "av_overview_template",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "av_overview_template",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValueSql: "''");

            migrationBuilder.AddColumn<bool>(
                name: "is_enabled",
                table: "av_overview_template",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "av_overview_template",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "sort_order",
                table: "av_overview_template",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "template",
                table: "av_overview_template",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "time",
                table: "av_overview_section_item",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(8)",
                oldMaxLength: 8);

            migrationBuilder.AlterColumn<string>(
                name: "summary",
                table: "av_overview_section_item",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<int>(
                name: "sort_order",
                table: "av_overview_section_item",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "av_overview_section_item",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValueSql: "''");

            migrationBuilder.AddColumn<bool>(
                name: "is_enabled",
                table: "av_overview_section_item",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "av_overview_section_item",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "start_time",
                table: "av_overview_section",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(8)",
                oldMaxLength: 8);

            migrationBuilder.AlterColumn<int>(
                name: "sort_order",
                table: "av_overview_section",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "other_source",
                table: "av_overview_section",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "av_overview_section",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "anchors",
                table: "av_overview_section",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "av_overview_template_id",
                table: "av_overview_section",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "av_overview_section",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValueSql: "''");

            migrationBuilder.AddColumn<bool>(
                name: "is_enabled",
                table: "av_overview_section",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "published_on",
                table: "av_overview_instance",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "av_overview_instance",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValueSql: "''");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "av_overview_instance",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "sort_order",
                table: "av_overview_instance",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_av_overview_template",
                table: "av_overview_template",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_avoverviewtemplatesection_is_enabled",
                table: "av_overview_template_section",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_template_name",
                table: "av_overview_template",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_avoverviewtemplate_is_enabled",
                table: "av_overview_template",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_avoverviewsectionitem_is_enabled",
                table: "av_overview_section_item",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_avoverviewsection_is_enabled",
                table: "av_overview_section",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_avoverviewinstance_is_enabled",
                table: "av_overview_instance",
                columns: new[] { "is_enabled", "name" });
            PostDown(migrationBuilder);
        }
    }
}
