using System;
using TNO.DAL;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1083 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.DropColumn(
                name: "template",
                table: "notification");

            migrationBuilder.RenameColumn(
                name: "require_alert",
                table: "notification",
                newName: "alert_on_index");

            migrationBuilder.RenameColumn(
                name: "filter",
                table: "notification",
                newName: "query");

            migrationBuilder.AddColumn<string>(
                name: "body",
                table: "notification_instance",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "owner_id",
                table: "notification_instance",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "subject",
                table: "notification_instance",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "owner_id",
                table: "notification",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "notification_template_id",
                table: "notification",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "notification_template",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    subject = table.Column<string>(type: "text", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    settings = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
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
                    table.PrimaryKey("PK_notification_template", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notification_notification_template_id",
                table: "notification",
                column: "notification_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_template_is_public_is_enabled",
                table: "notification_template",
                columns: new[] { "is_public", "is_enabled" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_template_name",
                table: "notification_template",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notificationtemplate_is_enabled",
                table: "notification_template",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.AddForeignKey(
                name: "FK_notification_notification_template_notification_template_id",
                table: "notification",
                column: "notification_template_id",
                principalTable: "notification_template",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropForeignKey(
                name: "FK_notification_notification_template_notification_template_id",
                table: "notification");

            migrationBuilder.DropTable(
                name: "notification_template");

            migrationBuilder.DropIndex(
                name: "IX_notification_notification_template_id",
                table: "notification");

            migrationBuilder.DropColumn(
                name: "body",
                table: "notification_instance");

            migrationBuilder.DropColumn(
                name: "owner_id",
                table: "notification_instance");

            migrationBuilder.DropColumn(
                name: "subject",
                table: "notification_instance");

            migrationBuilder.DropColumn(
                name: "notification_template_id",
                table: "notification");

            migrationBuilder.RenameColumn(
                name: "query",
                table: "notification",
                newName: "filter");

            migrationBuilder.RenameColumn(
                name: "alert_on_index",
                table: "notification",
                newName: "require_alert");

            migrationBuilder.AlterColumn<int>(
                name: "owner_id",
                table: "notification",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "template",
                table: "notification",
                type: "text",
                nullable: false,
                defaultValue: "");
            PostDown(migrationBuilder);
        }
    }
}
