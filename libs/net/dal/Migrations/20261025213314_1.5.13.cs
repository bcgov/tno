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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
