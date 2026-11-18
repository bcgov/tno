using System;
using TNO.DAL;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _102 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.DropTable(
                name: "print_content");

            migrationBuilder.DropIndex(
                name: "IX_work_order_work_type_status_created_on",
                table: "work_order");

            migrationBuilder.DropIndex(
                name: "IX_ingest_ingest_type_id",
                table: "ingest");

            migrationBuilder.DropIndex(
                name: "IX_content_content_type_source_uid_page_status",
                table: "content");

            migrationBuilder.RenameIndex(
                name: "IX_user_username",
                table: "user",
                newName: "IX_username");

            migrationBuilder.RenameIndex(
                name: "IX_user_key",
                table: "user",
                newName: "IX_key");

            migrationBuilder.RenameIndex(
                name: "IX_user_email",
                table: "user",
                newName: "IX_email");

            migrationBuilder.RenameIndex(
                name: "IX_tone_pool_name",
                table: "tone_pool",
                newName: "IX_name13");

            migrationBuilder.RenameIndex(
                name: "IX_tag_name",
                table: "tag",
                newName: "IX_name12");

            migrationBuilder.RenameIndex(
                name: "IX_source_action_name",
                table: "source_action",
                newName: "IX_name11");

            migrationBuilder.RenameIndex(
                name: "IX_source_name",
                table: "source",
                newName: "IX_name10");

            migrationBuilder.RenameIndex(
                name: "IX_source_code",
                table: "source",
                newName: "IX_code");

            migrationBuilder.RenameIndex(
                name: "IX_series_name",
                table: "series",
                newName: "IX_name9");

            migrationBuilder.RenameIndex(
                name: "IX_product_name",
                table: "product",
                newName: "IX_name8");

            migrationBuilder.RenameIndex(
                name: "IX_metric_name",
                table: "metric",
                newName: "IX_name7");

            migrationBuilder.RenameIndex(
                name: "IX_license_name",
                table: "license",
                newName: "IX_name6");

            migrationBuilder.RenameIndex(
                name: "IX_ingest_type_name",
                table: "ingest_type",
                newName: "IX_name5");

            migrationBuilder.RenameIndex(
                name: "IX_ingest_name",
                table: "ingest",
                newName: "IX_name4");

            migrationBuilder.RenameIndex(
                name: "IX_data_location_name",
                table: "data_location",
                newName: "IX_name3");

            migrationBuilder.RenameIndex(
                name: "IX_content_reference_published_on_partition_offset_status",
                table: "content_reference",
                newName: "IX_content_reference");

            migrationBuilder.RenameIndex(
                name: "IX_content_label_key_value",
                table: "content_label",
                newName: "IX_content_label");

            migrationBuilder.RenameIndex(
                name: "IX_content_published_on_created_on",
                table: "content",
                newName: "IX_content_dates");

            migrationBuilder.RenameIndex(
                name: "IX_connection_name",
                table: "connection",
                newName: "IX_name2");

            migrationBuilder.RenameIndex(
                name: "IX_category_name",
                table: "category",
                newName: "IX_name1");

            migrationBuilder.RenameIndex(
                name: "IX_action_name",
                table: "action",
                newName: "IX_name");

            migrationBuilder.AlterColumn<string>(
                name: "value",
                table: "content_action",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "byline",
                table: "content",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "edition",
                table: "content",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "section",
                table: "content",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_work_order",
                table: "work_order",
                columns: new[] { "work_type", "status", "created_on", "requestor_id", "assigned_id" });

            migrationBuilder.CreateIndex(
                name: "IX_last_first_name",
                table: "user",
                columns: new[] { "last_name", "first_name" });

            migrationBuilder.CreateIndex(
                name: "IX_schedule",
                table: "schedule",
                columns: new[] { "name", "is_enabled", "schedule_type" });

            migrationBuilder.CreateIndex(
                name: "IX_ingest",
                table: "ingest",
                columns: new[] { "ingest_type_id", "source_id", "topic" });

            migrationBuilder.CreateIndex(
                name: "IX_source_uid",
                table: "content_reference",
                columns: new[] { "source", "uid" });

            migrationBuilder.CreateIndex(
                name: "IX_content",
                table: "content",
                columns: new[] { "content_type", "source", "uid", "page", "status", "is_hidden" });

            migrationBuilder.CreateIndex(
                name: "IX_headline",
                table: "content",
                column: "headline");

            migrationBuilder.CreateIndex(
                name: "IX_print_content",
                table: "content",
                columns: new[] { "edition", "section", "byline" });

            migrationBuilder.CreateIndex(
                name: "IX_cache",
                table: "cache",
                columns: new[] { "key", "value" });

            migrationBuilder.CreateIndex(
                name: "IX_action",
                table: "action",
                columns: new[] { "value_type", "value_label" });
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropIndex(
                name: "IX_work_order",
                table: "work_order");

            migrationBuilder.DropIndex(
                name: "IX_last_first_name",
                table: "user");

            migrationBuilder.DropIndex(
                name: "IX_schedule",
                table: "schedule");

            migrationBuilder.DropIndex(
                name: "IX_ingest",
                table: "ingest");

            migrationBuilder.DropIndex(
                name: "IX_source_uid",
                table: "content_reference");

            migrationBuilder.DropIndex(
                name: "IX_content",
                table: "content");

            migrationBuilder.DropIndex(
                name: "IX_headline",
                table: "content");

            migrationBuilder.DropIndex(
                name: "IX_print_content",
                table: "content");

            migrationBuilder.DropIndex(
                name: "IX_cache",
                table: "cache");

            migrationBuilder.DropIndex(
                name: "IX_action",
                table: "action");

            migrationBuilder.DropColumn(
                name: "byline",
                table: "content");

            migrationBuilder.DropColumn(
                name: "edition",
                table: "content");

            migrationBuilder.DropColumn(
                name: "section",
                table: "content");

            migrationBuilder.RenameIndex(
                name: "IX_username",
                table: "user",
                newName: "IX_user_username");

            migrationBuilder.RenameIndex(
                name: "IX_key",
                table: "user",
                newName: "IX_user_key");

            migrationBuilder.RenameIndex(
                name: "IX_email",
                table: "user",
                newName: "IX_user_email");

            migrationBuilder.RenameIndex(
                name: "IX_name13",
                table: "tone_pool",
                newName: "IX_tone_pool_name");

            migrationBuilder.RenameIndex(
                name: "IX_name12",
                table: "tag",
                newName: "IX_tag_name");

            migrationBuilder.RenameIndex(
                name: "IX_name11",
                table: "source_action",
                newName: "IX_source_action_name");

            migrationBuilder.RenameIndex(
                name: "IX_name10",
                table: "source",
                newName: "IX_source_name");

            migrationBuilder.RenameIndex(
                name: "IX_code",
                table: "source",
                newName: "IX_source_code");

            migrationBuilder.RenameIndex(
                name: "IX_name9",
                table: "series",
                newName: "IX_series_name");

            migrationBuilder.RenameIndex(
                name: "IX_name8",
                table: "product",
                newName: "IX_product_name");

            migrationBuilder.RenameIndex(
                name: "IX_name7",
                table: "metric",
                newName: "IX_metric_name");

            migrationBuilder.RenameIndex(
                name: "IX_name6",
                table: "license",
                newName: "IX_license_name");

            migrationBuilder.RenameIndex(
                name: "IX_name5",
                table: "ingest_type",
                newName: "IX_ingest_type_name");

            migrationBuilder.RenameIndex(
                name: "IX_name4",
                table: "ingest",
                newName: "IX_ingest_name");

            migrationBuilder.RenameIndex(
                name: "IX_name3",
                table: "data_location",
                newName: "IX_data_location_name");

            migrationBuilder.RenameIndex(
                name: "IX_content_reference",
                table: "content_reference",
                newName: "IX_content_reference_published_on_partition_offset_status");

            migrationBuilder.RenameIndex(
                name: "IX_content_label",
                table: "content_label",
                newName: "IX_content_label_key_value");

            migrationBuilder.RenameIndex(
                name: "IX_content_dates",
                table: "content",
                newName: "IX_content_published_on_created_on");

            migrationBuilder.RenameIndex(
                name: "IX_name2",
                table: "connection",
                newName: "IX_connection_name");

            migrationBuilder.RenameIndex(
                name: "IX_name1",
                table: "category",
                newName: "IX_category_name");

            migrationBuilder.RenameIndex(
                name: "IX_name",
                table: "action",
                newName: "IX_action_name");

            migrationBuilder.AlterColumn<string>(
                name: "value",
                table: "content_action",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.CreateTable(
                name: "print_content",
                columns: table => new
                {
                    contentid = table.Column<long>(name: "content_id", type: "bigint", nullable: false),
                    byline = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    createdby = table.Column<string>(name: "created_by", type: "character varying(250)", maxLength: 250, nullable: false),
                    createdon = table.Column<DateTime>(name: "created_on", type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    edition = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    section = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updatedby = table.Column<string>(name: "updated_by", type: "character varying(250)", maxLength: 250, nullable: false),
                    updatedon = table.Column<DateTime>(name: "updated_on", type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_print_content", x => x.contentid);
                    table.ForeignKey(
                        name: "FK_print_content_content_content_id",
                        column: x => x.contentid,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_work_order_work_type_status_created_on",
                table: "work_order",
                columns: new[] { "work_type", "status", "created_on" });

            migrationBuilder.CreateIndex(
                name: "IX_ingest_ingest_type_id",
                table: "ingest",
                column: "ingest_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_content_type_source_uid_page_status",
                table: "content",
                columns: new[] { "content_type", "source", "uid", "page", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_print_content_edition_section",
                table: "print_content",
                columns: new[] { "edition", "section" });
            PostDown(migrationBuilder);
        }
    }
}
