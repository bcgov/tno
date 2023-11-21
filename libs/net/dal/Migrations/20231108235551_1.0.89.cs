using System;
using TNO.DAL;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1089 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);

            migrationBuilder.DropForeignKey(
                name: "FK_content_product_product_id",
                table: "content");

            migrationBuilder.DropForeignKey(
                name: "FK_ingest_product_product_id",
                table: "ingest");

            migrationBuilder.DropForeignKey(
                name: "FK_source_product_product_id",
                table: "source");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product",
                table: "product");

            migrationBuilder.RenameTable("product","public","media_type","public");

            migrationBuilder.AddPrimaryKey(
                name: "PK_media_type",
                table: "media_type",
                column: "id");

            migrationBuilder.RenameIndex(
                name: "IX_product_is_enabled",
                table: "media_type",
                newName: "IX_media_type_is_enabled");

            migrationBuilder.RenameColumn(
                name: "product_search_group_id",
                table: "source",
                newName: "media_type_search_group_id");

            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "source",
                newName: "media_type_id");

            migrationBuilder.RenameIndex(
                name: "IX_source_product_id",
                table: "source",
                newName: "IX_source_media_type_id");

            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "ingest",
                newName: "media_type_id");

            migrationBuilder.RenameIndex(
                name: "IX_ingest_product_id",
                table: "ingest",
                newName: "IX_ingest_media_type_id");

            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "content",
                newName: "media_type_id");

            migrationBuilder.RenameIndex(
                name: "IX_content_product_id",
                table: "content",
                newName: "IX_content_media_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_content_media_type_media_type_id",
                table: "content",
                column: "media_type_id",
                principalTable: "media_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ingest_media_type_media_type_id",
                table: "ingest",
                column: "media_type_id",
                principalTable: "media_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_source_media_type_media_type_id",
                table: "source",
                column: "media_type_id",
                principalTable: "media_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);

            migrationBuilder.DropForeignKey(
                name: "FK_content_media_type_media_type_id",
                table: "content");

            migrationBuilder.DropForeignKey(
                name: "FK_ingest_media_type_media_type_id",
                table: "ingest");

            migrationBuilder.DropForeignKey(
                name: "FK_source_media_type_media_type_id",
                table: "source");

            migrationBuilder.DropPrimaryKey(
                name: "PK_media_type",
                table: "media_type");

            migrationBuilder.RenameTable("media_type","public","product","public");

            migrationBuilder.AddPrimaryKey(
                name: "PK_product",
                table: "product",
                column: "id");

            migrationBuilder.RenameIndex(
                name: "IX_media_type_is_enabled",
                table: "media_type",
                newName: "IX_product_is_enabled");

            migrationBuilder.RenameColumn(
                name: "media_type_search_group_id",
                table: "source",
                newName: "product_search_group_id");

            migrationBuilder.RenameColumn(
                name: "media_type_id",
                table: "source",
                newName: "product_id");

            migrationBuilder.RenameIndex(
                name: "IX_source_media_type_id",
                table: "source",
                newName: "IX_source_product_id");

            migrationBuilder.RenameColumn(
                name: "media_type_id",
                table: "ingest",
                newName: "product_id");

            migrationBuilder.RenameIndex(
                name: "IX_ingest_media_type_id",
                table: "ingest",
                newName: "IX_ingest_product_id");

            migrationBuilder.RenameColumn(
                name: "media_type_id",
                table: "content",
                newName: "product_id");

            migrationBuilder.RenameIndex(
                name: "IX_content_media_type_id",
                table: "content",
                newName: "IX_content_product_id");

            migrationBuilder.AddForeignKey(
                name: "FK_content_product_product_id",
                table: "content",
                column: "product_id",
                principalTable: "product",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ingest_product_product_id",
                table: "ingest",
                column: "product_id",
                principalTable: "product",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_source_product_product_id",
                table: "source",
                column: "product_id",
                principalTable: "product",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            PostDown(migrationBuilder);
        }
    }
}
