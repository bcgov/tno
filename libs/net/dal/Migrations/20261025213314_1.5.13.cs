using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1513 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_content_contributor_contributor_id",
                table: "content");

            migrationBuilder.DropForeignKey(
                name: "FK_content_license_license_id",
                table: "content");

            migrationBuilder.DropForeignKey(
                name: "FK_content_media_type_media_type_id",
                table: "content");

            migrationBuilder.DropForeignKey(
                name: "FK_content_series_series_id",
                table: "content");

            migrationBuilder.DropForeignKey(
                name: "FK_content_source_source_id",
                table: "content");

            migrationBuilder.AddForeignKey(
                name: "FK_content_contributor_contributor_id",
                table: "content",
                column: "contributor_id",
                principalTable: "contributor",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_content_license_license_id",
                table: "content",
                column: "license_id",
                principalTable: "license",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_content_media_type_media_type_id",
                table: "content",
                column: "media_type_id",
                principalTable: "media_type",
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

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "orphan_scan",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_slot = table.Column<int>(type: "integer", nullable: false),
                    side = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    index = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    next_batch_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    start_posted_on = table.Column<DateOnly>(type: "date", nullable: true),
                    end_posted_on = table.Column<DateOnly>(type: "date", nullable: true),
                    after_posted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    min_id = table.Column<long>(type: "bigint", nullable: false),
                    max_id = table.Column<long>(type: "bigint", nullable: true),
                    after = table.Column<long>(type: "bigint", nullable: false),
                    scanned = table.Column<long>(type: "bigint", nullable: false),
                    pending = table.Column<long>(type: "bigint", nullable: false),
                    found = table.Column<long>(type: "bigint", nullable: false),
                    cancel_requested = table.Column<bool>(type: "boolean", nullable: false),
                    pit_id = table.Column<string>(type: "text", nullable: true),
                    search_after = table.Column<string>(type: "text", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orphan_scan", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "orphan_scan_result",
                schema: "public",
                columns: table => new
                {
                    scan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    detected_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    headline = table.Column<string>(type: "text", nullable: false),
                    resolution = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orphan_scan_result", x => new { x.scan_id, x.content_id });
                    table.ForeignKey(
                        name: "FK_orphan_scan_result_orphan_scan_scan_id",
                        column: x => x.scan_id,
                        principalSchema: "public",
                        principalTable: "orphan_scan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_orphan_scan_created_on",
                schema: "public",
                table: "orphan_scan",
                column: "created_on");

            migrationBuilder.CreateIndex(
                name: "IX_orphan_scan_worker_slot",
                schema: "public",
                table: "orphan_scan",
                column: "worker_slot",
                unique: true,
                filter: "completed_on IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_orphan_scan_result_scan_id_confirmed_content_id",
                schema: "public",
                table: "orphan_scan_result",
                columns: new[] { "scan_id", "confirmed", "content_id" });

            migrationBuilder.CreateIndex(
                name: "IX_orphan_scan_result_scan_id_confirmed_detected_on",
                schema: "public",
                table: "orphan_scan_result",
                columns: new[] { "scan_id", "confirmed", "detected_on" });

            migrationBuilder.CreateIndex(
                name: "IX_content_posted_on_id",
                table: "content",
                columns: new[] { "posted_on", "id" })
                .Annotation("Npgsql:CreatedConcurrently", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_content_posted_on_id",
                table: "content");

            migrationBuilder.DropTable(
                name: "orphan_scan_result",
                schema: "public");

            migrationBuilder.DropTable(
                name: "orphan_scan",
                schema: "public");

            migrationBuilder.DropForeignKey(
                name: "FK_content_contributor_contributor_id",
                table: "content");

            migrationBuilder.DropForeignKey(
                name: "FK_content_license_license_id",
                table: "content");

            migrationBuilder.DropForeignKey(
                name: "FK_content_media_type_media_type_id",
                table: "content");

            migrationBuilder.DropForeignKey(
                name: "FK_content_series_series_id",
                table: "content");

            migrationBuilder.DropForeignKey(
                name: "FK_content_source_source_id",
                table: "content");

            migrationBuilder.AddForeignKey(
                name: "FK_content_contributor_contributor_id",
                table: "content",
                column: "contributor_id",
                principalTable: "contributor",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_content_license_license_id",
                table: "content",
                column: "license_id",
                principalTable: "license",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_content_media_type_media_type_id",
                table: "content",
                column: "media_type_id",
                principalTable: "media_type",
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
        }
    }
}
