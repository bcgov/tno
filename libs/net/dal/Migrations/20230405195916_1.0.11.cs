using System;
using TNO.DAL;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1011 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.DropForeignKey(
                name: "FK_report_instance_report_ReportId",
                table: "report_instance");

            migrationBuilder.DropIndex(
                name: "IX_name8",
                table: "report");

            migrationBuilder.RenameIndex(
                name: "IX_name9",
                table: "source",
                newName: "IX_name8");

            migrationBuilder.RenameIndex(
                name: "IX_name10",
                table: "tag",
                newName: "IX_name9");

            migrationBuilder.RenameIndex(
                name: "IX_name11",
                table: "tone_pool",
                newName: "IX_name10");

            migrationBuilder.RenameIndex(
                name: "IX_name12",
                table: "topic",
                newName: "IX_name11");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled12",
                table: "topic",
                newName: "IX_is_enabled13");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled11",
                table: "tone_pool",
                newName: "IX_is_enabled12");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled10",
                table: "tag",
                newName: "IX_is_enabled11");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled9",
                table: "source",
                newName: "IX_is_enabled10");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled8",
                table: "series",
                newName: "IX_is_enabled9");

            migrationBuilder.RenameColumn(
                name: "ReportId",
                table: "report_instance",
                newName: "report_id");

            migrationBuilder.RenameColumn(
                name: "PublishedOn",
                table: "report_instance",
                newName: "published_on");

            migrationBuilder.RenameIndex(
                name: "IX_report_instance_ReportId",
                table: "report_instance",
                newName: "IX_report_instance_report_id");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled7",
                table: "report",
                newName: "IX_is_enabled8");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled6",
                table: "product",
                newName: "IX_is_enabled7");

            migrationBuilder.AddColumn<JsonDocument>(
                name: "response",
                table: "report_instance",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb");

            migrationBuilder.AddColumn<bool>(
                name: "is_public",
                table: "report",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "owner_id",
                table: "report",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "settings",
                table: "report",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb");

            migrationBuilder.AddColumn<string>(
                name: "template",
                table: "report",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    notification_type = table.Column<int>(type: "integer", nullable: false),
                    require_alert = table.Column<bool>(type: "boolean", nullable: false),
                    filter = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    resend = table.Column<int>(type: "integer", nullable: false),
                    owner_id = table.Column<int>(type: "integer", nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    settings = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    template = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, defaultValueSql: "''"),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification", x => x.id);
                    table.ForeignKey(
                        name: "FK_notification_user_owner_id",
                        column: x => x.owner_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_report",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    report_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_report", x => new { x.user_id, x.report_id });
                    table.ForeignKey(
                        name: "FK_user_report_report_report_id",
                        column: x => x.report_id,
                        principalTable: "report",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_report_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification_instance",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    notification_id = table.Column<int>(type: "integer", nullable: false),
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    response = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_instance", x => x.id);
                    table.ForeignKey(
                        name: "FK_notification_instance_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notification_instance_notification_notification_id",
                        column: x => x.notification_id,
                        principalTable: "notification",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_notification",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    notification_id = table.Column<int>(type: "integer", nullable: false),
                    resend = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_notification", x => new { x.user_id, x.notification_id });
                    table.ForeignKey(
                        name: "FK_user_notification_notification_notification_id",
                        column: x => x.notification_id,
                        principalTable: "notification",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_notification_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_report_owner_id_name",
                table: "report",
                columns: new[] { "owner_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_is_enabled6",
                table: "notification",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_owner_id_name",
                table: "notification",
                columns: new[] { "owner_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_instance_content_id",
                table: "notification_instance",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_instance_notification_id",
                table: "notification_instance",
                column: "notification_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_notification_notification_id",
                table: "user_notification",
                column: "notification_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_report_report_id",
                table: "user_report",
                column: "report_id");

            migrationBuilder.AddForeignKey(
                name: "FK_report_user_owner_id",
                table: "report",
                column: "owner_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_report_instance_report_report_id",
                table: "report_instance",
                column: "report_id",
                principalTable: "report",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropForeignKey(
                name: "FK_report_user_owner_id",
                table: "report");

            migrationBuilder.DropForeignKey(
                name: "FK_report_instance_report_report_id",
                table: "report_instance");

            migrationBuilder.DropTable(
                name: "notification_instance");

            migrationBuilder.DropTable(
                name: "user_notification");

            migrationBuilder.DropTable(
                name: "user_report");

            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropIndex(
                name: "IX_report_owner_id_name",
                table: "report");

            migrationBuilder.DropColumn(
                name: "response",
                table: "report_instance");

            migrationBuilder.DropColumn(
                name: "is_public",
                table: "report");

            migrationBuilder.DropColumn(
                name: "owner_id",
                table: "report");

            migrationBuilder.DropColumn(
                name: "settings",
                table: "report");

            migrationBuilder.DropColumn(
                name: "template",
                table: "report");

            migrationBuilder.RenameIndex(
                name: "IX_name11",
                table: "topic",
                newName: "IX_name12");

            migrationBuilder.RenameIndex(
                name: "IX_name10",
                table: "tone_pool",
                newName: "IX_name11");

            migrationBuilder.RenameIndex(
                name: "IX_name9",
                table: "tag",
                newName: "IX_name10");

            migrationBuilder.RenameIndex(
                name: "IX_name8",
                table: "source",
                newName: "IX_name9");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled7",
                table: "product",
                newName: "IX_is_enabled6");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled8",
                table: "report",
                newName: "IX_is_enabled7");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled9",
                table: "series",
                newName: "IX_is_enabled8");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled10",
                table: "source",
                newName: "IX_is_enabled9");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled11",
                table: "tag",
                newName: "IX_is_enabled10");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled12",
                table: "tone_pool",
                newName: "IX_is_enabled11");

            migrationBuilder.RenameIndex(
                name: "IX_is_enabled13",
                table: "topic",
                newName: "IX_is_enabled12");

            migrationBuilder.RenameColumn(
                name: "report_id",
                table: "report_instance",
                newName: "ReportId");

            migrationBuilder.RenameColumn(
                name: "published_on",
                table: "report_instance",
                newName: "PublishedOn");

            migrationBuilder.RenameIndex(
                name: "IX_report_instance_report_id",
                table: "report_instance",
                newName: "IX_report_instance_ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_name8",
                table: "report",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_report_instance_report_ReportId",
                table: "report_instance",
                column: "ReportId",
                principalTable: "report",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
            PostDown(migrationBuilder);
        }
    }
}
