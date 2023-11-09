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

            // migrationBuilder.DropTable(
            //     name: "product");

            migrationBuilder.RenameTable("product","media_type");

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

            migrationBuilder.RenameIndex(
                name: "IX_name6",
                table: "metric",
                newName: "IX_name7");

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

            // migrationBuilder.CreateTable(
            //     name: "media_type",
            //     columns: table => new
            //     {
            //         id = table.Column<int>(type: "integer", nullable: false)
            //             .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //         auto_transcribe = table.Column<bool>(type: "boolean", nullable: false),
            //         Settings = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
            //         created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
            //         created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
            //         updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
            //         updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
            //         version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0"),
            //         name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
            //         description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, defaultValueSql: "''"),
            //         is_enabled = table.Column<bool>(type: "boolean", nullable: false),
            //         sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_media_type", x => x.id);
            //     });

            // migrationBuilder.CreateIndex(
            //     name: "IX_mediatype_is_enabled",
            //     table: "media_type",
            //     columns: new[] { "is_enabled", "name" });

            // migrationBuilder.CreateIndex(
            //     name: "IX_name6",
            //     table: "media_type",
            //     column: "name",
            //     unique: true);

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

        //    migrationBuilder.DropTable(
        //        name: "media_type");

            migrationBuilder.RenameTable("media_type","product");
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

            migrationBuilder.RenameIndex(
                name: "IX_name7",
                table: "metric",
                newName: "IX_name6");

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

            // migrationBuilder.CreateTable(
            //     name: "product",
            //     columns: table => new
            //     {
            //         id = table.Column<int>(type: "integer", nullable: false)
            //             .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //         auto_transcribe = table.Column<bool>(type: "boolean", nullable: false),
            //         created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
            //         created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
            //         description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, defaultValueSql: "''"),
            //         is_enabled = table.Column<bool>(type: "boolean", nullable: false),
            //         name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
            //         Settings = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
            //         sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
            //         updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
            //         updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
            //         version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_product", x => x.id);
            //     });

            // migrationBuilder.CreateIndex(
            //     name: "IX_name7",
            //     table: "product",
            //     column: "name",
            //     unique: true);

            // migrationBuilder.CreateIndex(
            //     name: "IX_product_is_enabled",
            //     table: "product",
            //     columns: new[] { "is_enabled", "name" });

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
