using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1516 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);

            migrationBuilder.CreateTable(
                name: "report_instance_chart",
                columns: table => new
                {
                    instance_id = table.Column<long>(type: "bigint", nullable: false),
                    section_id = table.Column<int>(type: "integer", nullable: false),
                    chart_template_id = table.Column<int>(type: "integer", nullable: false),
                    section_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    data = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    doc_count = table.Column<long>(type: "bigint", nullable: false),
                    is_truncated = table.Column<bool>(type: "boolean", nullable: false),
                    generated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_instance_chart", x => new { x.instance_id, x.section_id, x.chart_template_id });
                    table.ForeignKey(
                        name: "FK_report_instance_chart_chart_template_chart_template_id",
                        column: x => x.chart_template_id,
                        principalTable: "chart_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_instance_chart_report_instance_instance_id",
                        column: x => x.instance_id,
                        principalTable: "report_instance",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_instance_chart_report_section_section_id",
                        column: x => x.section_id,
                        principalTable: "report_section",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_report_instance_chart_chart_template_id",
                table: "report_instance_chart",
                column: "chart_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_instance_chart_section_id",
                table: "report_instance_chart",
                column: "section_id");

            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);

            migrationBuilder.DropTable(
                name: "report_instance_chart");

            PostDown(migrationBuilder);
        }
    }
}
