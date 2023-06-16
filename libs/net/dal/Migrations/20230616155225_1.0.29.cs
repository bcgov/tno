using System;
using TNO.DAL;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1029 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.DropIndex(
                name: "IX_schedule",
                table: "schedule");

            migrationBuilder.DropColumn(
                name: "schedule_type",
                table: "schedule");

            migrationBuilder.AlterColumn<bool>(
                name: "repeat",
                table: "schedule",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "event_schedule",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    schedule_id = table.Column<int>(type: "integer", nullable: false),
                    event_type = table.Column<int>(type: "integer", nullable: false),
                    settings = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    last_ran_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_schedule", x => x.id);
                    table.ForeignKey(
                        name: "FK_event_schedule_schedule_schedule_id",
                        column: x => x.schedule_id,
                        principalTable: "schedule",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_schedule",
                table: "schedule",
                columns: new[] { "name", "is_enabled" });

            migrationBuilder.CreateIndex(
                name: "IX_event_schedule_schedule_id",
                table: "event_schedule",
                column: "schedule_id");
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropTable(
                name: "event_schedule");

            migrationBuilder.DropIndex(
                name: "IX_schedule",
                table: "schedule");

            migrationBuilder.AlterColumn<int>(
                name: "repeat",
                table: "schedule",
                type: "integer",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<int>(
                name: "schedule_type",
                table: "schedule",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_schedule",
                table: "schedule",
                columns: new[] { "name", "is_enabled", "schedule_type" });
            PostDown(migrationBuilder);
        }
    }
}
