using System;
using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _158 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);

            migrationBuilder.CreateTable(
                name: "report_ai_test",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    report_id = table.Column<int>(type: "integer", nullable: false),
                    report_instance_id = table.Column<long>(type: "bigint", nullable: false),
                    report_section_id = table.Column<int>(type: "integer", nullable: false),
                    requestor_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    output = table.Column<string>(type: "text", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true),
                    request_count = table.Column<int>(type: "integer", nullable: false),
                    prompt_tokens = table.Column<int>(type: "integer", nullable: false),
                    completion_tokens = table.Column<int>(type: "integer", nullable: false),
                    duration_ms = table.Column<long>(type: "bigint", nullable: false),
                    story_count = table.Column<int>(type: "integer", nullable: false),
                    reduction_depth = table.Column<int>(type: "integer", nullable: false),
                    transcript = table.Column<string>(type: "text", nullable: false),
                    completed_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_ai_test", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_ai_test_report_instance_report_instance_id",
                        column: x => x.report_instance_id,
                        principalTable: "report_instance",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_ai_test_report_report_id",
                        column: x => x.report_id,
                        principalTable: "report",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_ai_test_report_section_report_section_id",
                        column: x => x.report_section_id,
                        principalTable: "report_section",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_ai_test_user_requestor_id",
                        column: x => x.requestor_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_report_ai_test_created_on",
                table: "report_ai_test",
                column: "created_on");

            migrationBuilder.CreateIndex(
                name: "IX_report_ai_test_report_id",
                table: "report_ai_test",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_ai_test_report_instance_id",
                table: "report_ai_test",
                column: "report_instance_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_ai_test_report_section_id",
                table: "report_ai_test",
                column: "report_section_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_ai_test_requestor_section",
                table: "report_ai_test",
                columns: new[] { "requestor_id", "report_section_id" });

            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);

            migrationBuilder.DropTable(
                name: "report_ai_test");

            PostDown(migrationBuilder);
        }
    }
}
