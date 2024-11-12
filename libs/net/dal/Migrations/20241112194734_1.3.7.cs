using System;
using TNO.DAL;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TNO.Entities.Models;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _137 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.CreateTable(
                name: "action",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    value_label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "''"),
                    value_type = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    default_value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "''"),
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
                    table.PrimaryKey("PK_action", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cache",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    value = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, defaultValueSql: "''"),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cache", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "chart_template",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    template = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_chart_template", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "connection",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    connection_type = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    configuration = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    is_read_only = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_connection", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "content_reference",
                columns: table => new
                {
                    source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    uid = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    topic = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    published_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    source_updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_reference", x => new { x.source, x.uid });
                });

            migrationBuilder.CreateTable(
                name: "ingest_type",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    content_type = table.Column<int>(type: "integer", nullable: false),
                    auto_transcribe = table.Column<bool>(type: "boolean", nullable: false),
                    disable_transcribe = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_ingest_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "license",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ttl = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_license", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "media_type",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    auto_transcribe = table.Column<bool>(type: "boolean", nullable: false),
                    list_option = table.Column<int>(type: "integer", nullable: false),
                    Settings = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
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
                    table.PrimaryKey("PK_media_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "metric",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    table.PrimaryKey("PK_metric", x => x.id);
                });

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

            migrationBuilder.CreateTable(
                name: "organization",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_organization", x => x.id);
                    table.ForeignKey(
                        name: "FK_organization_organization_parent_id",
                        column: x => x.parent_id,
                        principalTable: "organization",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_type = table.Column<int>(type: "integer", nullable: false),
                    target_product_id = table.Column<int>(type: "integer", nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_product", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "report_template",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    report_type = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "0"),
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
                    table.PrimaryKey("PK_report_template", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sentiment",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    value = table.Column<float>(type: "real", nullable: false),
                    rate = table.Column<float>(type: "real", nullable: false),
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
                    table.PrimaryKey("PK_sentiment", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "setting",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, defaultValueSql: "''"),
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
                    table.PrimaryKey("PK_setting", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "system_message",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    message = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_message", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tag",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
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
                    table.PrimaryKey("PK_tag", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "topic",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    topic_type = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_topic", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, defaultValueSql: "''"),
                    preferred_email = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, defaultValueSql: "''"),
                    key = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "''"),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "''"),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "''"),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    email_verified = table.Column<bool>(type: "boolean", nullable: false),
                    is_system_account = table.Column<bool>(type: "boolean", nullable: false),
                    account_type = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_login_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, defaultValueSql: "''"),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValueSql: "''"),
                    code_created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    preferences = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    unique_logins = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "0"),
                    roles = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, defaultValueSql: "''"),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "content_type_action",
                columns: table => new
                {
                    content_type = table.Column<int>(type: "integer", nullable: false),
                    action_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_type_action", x => new { x.content_type, x.action_id });
                    table.ForeignKey(
                        name: "FK_content_type_action_action_action_id",
                        column: x => x.action_id,
                        principalTable: "action",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "data_location",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    connection_id = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_data_location", x => x.id);
                    table.ForeignKey(
                        name: "FK_data_location_connection_connection_id",
                        column: x => x.connection_id,
                        principalTable: "connection",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "minister",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    aliases = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, defaultValueSql: "''"),
                    position = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false, defaultValueSql: "''"),
                    organization_id = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_minister", x => x.id);
                    table.ForeignKey(
                        name: "FK_minister_organization_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organization",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "av_overview_template",
                columns: table => new
                {
                    template_type = table.Column<int>(type: "integer", nullable: false),
                    report_template_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_av_overview_template", x => x.template_type);
                    table.ForeignKey(
                        name: "FK_av_overview_template_report_template_report_template_id",
                        column: x => x.report_template_id,
                        principalTable: "report_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_template_chart_template",
                columns: table => new
                {
                    report_template_id = table.Column<int>(type: "integer", nullable: false),
                    chart_template_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_template_chart_template", x => new { x.report_template_id, x.chart_template_id });
                    table.ForeignKey(
                        name: "FK_report_template_chart_template_chart_template_chart_templat~",
                        column: x => x.chart_template_id,
                        principalTable: "chart_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_template_chart_template_report_template_report_templ~",
                        column: x => x.report_template_id,
                        principalTable: "report_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "filter",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    owner_id = table.Column<int>(type: "integer", nullable: true),
                    query = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
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
                    table.PrimaryKey("PK_filter", x => x.id);
                    table.ForeignKey(
                        name: "FK_filter_user_owner_id",
                        column: x => x.owner_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    notification_type = table.Column<int>(type: "integer", nullable: false),
                    resend = table.Column<int>(type: "integer", nullable: false),
                    owner_id = table.Column<int>(type: "integer", nullable: true),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    alert_on_index = table.Column<bool>(type: "boolean", nullable: false),
                    notification_template_id = table.Column<int>(type: "integer", nullable: false),
                    settings = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    query = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
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
                        name: "FK_notification_notification_template_notification_template_id",
                        column: x => x.notification_template_id,
                        principalTable: "notification_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notification_user_owner_id",
                        column: x => x.owner_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    owner_id = table.Column<int>(type: "integer", nullable: true),
                    report_template_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_report", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_report_template_report_template_id",
                        column: x => x.report_template_id,
                        principalTable: "report_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_user_owner_id",
                        column: x => x.owner_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "schedule",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    delay_ms = table.Column<int>(type: "integer", nullable: false),
                    run_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    start_at = table.Column<TimeSpan>(type: "interval", nullable: true),
                    stop_at = table.Column<TimeSpan>(type: "interval", nullable: true),
                    run_only_once = table.Column<bool>(type: "boolean", nullable: false),
                    repeat = table.Column<bool>(type: "boolean", nullable: false),
                    run_on_week_days = table.Column<int>(type: "integer", nullable: false),
                    run_on_months = table.Column<int>(type: "integer", nullable: false),
                    day_of_month = table.Column<int>(type: "integer", nullable: false),
                    requested_by_id = table.Column<int>(type: "integer", nullable: true),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedule", x => x.id);
                    table.ForeignKey(
                        name: "FK_schedule_user_requested_by_id",
                        column: x => x.requested_by_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "source",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    short_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "''"),
                    license_id = table.Column<int>(type: "integer", nullable: false),
                    owner_id = table.Column<int>(type: "integer", nullable: true),
                    media_type_id = table.Column<int>(type: "integer", nullable: true),
                    auto_transcribe = table.Column<bool>(type: "boolean", nullable: false),
                    disable_transcribe = table.Column<bool>(type: "boolean", nullable: false),
                    use_in_topics = table.Column<bool>(type: "boolean", nullable: false),
                    configuration = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
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
                    table.PrimaryKey("PK_source", x => x.id);
                    table.ForeignKey(
                        name: "FK_source_license_license_id",
                        column: x => x.license_id,
                        principalTable: "license",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_source_media_type_media_type_id",
                        column: x => x.media_type_id,
                        principalTable: "media_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_source_user_owner_id",
                        column: x => x.owner_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tone_pool",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    owner_id = table.Column<int>(type: "integer", nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_tone_pool", x => x.id);
                    table.ForeignKey(
                        name: "FK_tone_pool_user_owner_id",
                        column: x => x.owner_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_colleague",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    colleague_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_colleague", x => new { x.user_id, x.colleague_id });
                    table.ForeignKey(
                        name: "FK_user_colleague_user_colleague_id",
                        column: x => x.colleague_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_colleague_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_distribution",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    linked_user_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_distribution", x => new { x.user_id, x.linked_user_id });
                    table.ForeignKey(
                        name: "FK_user_distribution_user_linked_user_id",
                        column: x => x.linked_user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_distribution_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_media_type",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    media_type_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_media_type", x => new { x.user_id, x.media_type_id });
                    table.ForeignKey(
                        name: "FK_user_media_type_media_type_media_type_id",
                        column: x => x.media_type_id,
                        principalTable: "media_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_media_type_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_organization",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    organization_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_organization", x => new { x.user_id, x.organization_id });
                    table.ForeignKey(
                        name: "FK_user_organization_organization_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organization",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_organization_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_product",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_product", x => new { x.user_id, x.product_id });
                    table.ForeignKey(
                        name: "FK_user_product_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_product_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "av_overview_instance",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    template_type = table.Column<int>(type: "integer", nullable: false),
                    published_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    response = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_av_overview_instance", x => x.id);
                    table.ForeignKey(
                        name: "FK_av_overview_instance_av_overview_template_template_type",
                        column: x => x.template_type,
                        principalTable: "av_overview_template",
                        principalColumn: "template_type",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_av_overview",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    av_overview_template_type = table.Column<int>(type: "integer", nullable: false),
                    is_subscribed = table.Column<bool>(type: "boolean", nullable: false),
                    send_to = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_av_overview", x => new { x.user_id, x.av_overview_template_type });
                    table.ForeignKey(
                        name: "FK_user_av_overview_av_overview_template_av_overview_template_~",
                        column: x => x.av_overview_template_type,
                        principalTable: "av_overview_template",
                        principalColumn: "template_type",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_av_overview_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "folder",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    owner_id = table.Column<int>(type: "integer", nullable: true),
                    filter_id = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_folder", x => x.id);
                    table.ForeignKey(
                        name: "FK_folder_filter_filter_id",
                        column: x => x.filter_id,
                        principalTable: "filter",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_folder_user_owner_id",
                        column: x => x.owner_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_notification",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    notification_id = table.Column<int>(type: "integer", nullable: false),
                    resend = table.Column<int>(type: "integer", nullable: true),
                    is_subscribed = table.Column<bool>(type: "boolean", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "report_instance",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    report_id = table.Column<int>(type: "integer", nullable: false),
                    owner_id = table.Column<int>(type: "integer", nullable: true),
                    published_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sent_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    subject = table.Column<string>(type: "text", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    response = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_instance", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_instance_report_report_id",
                        column: x => x.report_id,
                        principalTable: "report",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_instance_user_owner_id",
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
                    is_subscribed = table.Column<bool>(type: "boolean", nullable: false),
                    format = table.Column<int>(type: "integer", nullable: false),
                    send_to = table.Column<int>(type: "integer", nullable: false),
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
                name: "contributor",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source_id = table.Column<int>(type: "integer", nullable: true),
                    auto_transcribe = table.Column<bool>(type: "boolean", nullable: false),
                    is_press = table.Column<bool>(type: "boolean", nullable: false),
                    aliases = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_contributor", x => x.id);
                    table.ForeignKey(
                        name: "FK_contributor_source_source_id",
                        column: x => x.source_id,
                        principalTable: "source",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "earned_media",
                columns: table => new
                {
                    source_id = table.Column<int>(type: "integer", nullable: false),
                    content_type = table.Column<int>(type: "integer", nullable: false),
                    length_of_content = table.Column<int>(type: "integer", nullable: false),
                    rate = table.Column<float>(type: "real", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_earned_media", x => new { x.source_id, x.content_type });
                    table.ForeignKey(
                        name: "FK_earned_media_source_source_id",
                        column: x => x.source_id,
                        principalTable: "source",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ingest",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    topic = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, defaultValueSql: "''"),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    source_id = table.Column<int>(type: "integer", nullable: false),
                    ingest_type_id = table.Column<int>(type: "integer", nullable: false),
                    media_type_id = table.Column<int>(type: "integer", nullable: false),
                    source_connection_id = table.Column<int>(type: "integer", nullable: false),
                    destination_connection_id = table.Column<int>(type: "integer", nullable: false),
                    configuration = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    retry_limit = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    reset_retry_after_delay_ms = table.Column<int>(type: "integer", nullable: false),
                    schedule_type = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ingest", x => x.id);
                    table.ForeignKey(
                        name: "FK_ingest_connection_destination_connection_id",
                        column: x => x.destination_connection_id,
                        principalTable: "connection",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ingest_connection_source_connection_id",
                        column: x => x.source_connection_id,
                        principalTable: "connection",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ingest_ingest_type_ingest_type_id",
                        column: x => x.ingest_type_id,
                        principalTable: "ingest_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ingest_media_type_media_type_id",
                        column: x => x.media_type_id,
                        principalTable: "media_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ingest_source_source_id",
                        column: x => x.source_id,
                        principalTable: "source",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "series",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source_id = table.Column<int>(type: "integer", nullable: true),
                    auto_transcribe = table.Column<bool>(type: "boolean", nullable: false),
                    use_in_topics = table.Column<bool>(type: "boolean", nullable: false),
                    is_other = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_series", x => x.id);
                    table.ForeignKey(
                        name: "FK_series_source_source_id",
                        column: x => x.source_id,
                        principalTable: "source",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "source_media_type_search_mapping",
                columns: table => new
                {
                    source_id = table.Column<int>(type: "integer", nullable: false),
                    media_type_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_source_media_type_search_mapping", x => new { x.source_id, x.media_type_id });
                    table.ForeignKey(
                        name: "FK_source_media_type_search_mapping_media_type_media_type_id",
                        column: x => x.media_type_id,
                        principalTable: "media_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_source_media_type_search_mapping_source_source_id",
                        column: x => x.source_id,
                        principalTable: "source",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "source_metric",
                columns: table => new
                {
                    source_id = table.Column<int>(type: "integer", nullable: false),
                    metric_id = table.Column<int>(type: "integer", nullable: false),
                    reach = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    earned = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    impression = table.Column<float>(type: "real", nullable: false, defaultValue: 0f),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_source_metric", x => new { x.source_id, x.metric_id });
                    table.ForeignKey(
                        name: "FK_source_metric_metric_metric_id",
                        column: x => x.metric_id,
                        principalTable: "metric",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_source_metric_source_source_id",
                        column: x => x.source_id,
                        principalTable: "source",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_source",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    source_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_source", x => new { x.user_id, x.source_id });
                    table.ForeignKey(
                        name: "FK_user_source_source_source_id",
                        column: x => x.source_id,
                        principalTable: "source",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_source_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_av_overview_instance",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    report_instance_id = table.Column<long>(type: "bigint", nullable: false),
                    sent_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    response = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_av_overview_instance", x => new { x.user_id, x.report_instance_id });
                    table.ForeignKey(
                        name: "FK_user_av_overview_instance_av_overview_instance_report_insta~",
                        column: x => x.report_instance_id,
                        principalTable: "av_overview_instance",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_av_overview_instance_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_schedule",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    schedule_id = table.Column<int>(type: "integer", nullable: false),
                    event_type = table.Column<int>(type: "integer", nullable: false),
                    settings = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    request_sent_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_ran_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    report_id = table.Column<int>(type: "integer", nullable: true),
                    notification_id = table.Column<int>(type: "integer", nullable: true),
                    folder_id = table.Column<int>(type: "integer", nullable: true),
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
                        name: "FK_event_schedule_folder_folder_id",
                        column: x => x.folder_id,
                        principalTable: "folder",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_event_schedule_notification_notification_id",
                        column: x => x.notification_id,
                        principalTable: "notification",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_event_schedule_report_report_id",
                        column: x => x.report_id,
                        principalTable: "report",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_event_schedule_schedule_schedule_id",
                        column: x => x.schedule_id,
                        principalTable: "schedule",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_section",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    report_id = table.Column<int>(type: "integer", nullable: false),
                    section_type = table.Column<int>(type: "integer", nullable: false),
                    filter_id = table.Column<int>(type: "integer", nullable: true),
                    folder_id = table.Column<int>(type: "integer", nullable: true),
                    linked_report_id = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_report_section", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_section_filter_filter_id",
                        column: x => x.filter_id,
                        principalTable: "filter",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_section_folder_folder_id",
                        column: x => x.folder_id,
                        principalTable: "folder",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_section_report_linked_report_id",
                        column: x => x.linked_report_id,
                        principalTable: "report",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_section_report_report_id",
                        column: x => x.report_id,
                        principalTable: "report",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_report_instance",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    report_instance_id = table.Column<long>(type: "bigint", nullable: false),
                    link_status = table.Column<int>(type: "integer", nullable: false),
                    link_sent_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    link_response = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    text_status = table.Column<int>(type: "integer", nullable: false),
                    text_sent_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    text_response = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_report_instance", x => new { x.user_id, x.report_instance_id });
                    table.ForeignKey(
                        name: "FK_user_report_instance_report_instance_report_instance_id",
                        column: x => x.report_instance_id,
                        principalTable: "report_instance",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_report_instance_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ingest_data_location",
                columns: table => new
                {
                    ingest_id = table.Column<int>(type: "integer", nullable: false),
                    data_location_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ingest_data_location", x => new { x.ingest_id, x.data_location_id });
                    table.ForeignKey(
                        name: "FK_ingest_data_location_data_location_data_location_id",
                        column: x => x.data_location_id,
                        principalTable: "data_location",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ingest_data_location_ingest_ingest_id",
                        column: x => x.ingest_id,
                        principalTable: "ingest",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ingest_schedule",
                columns: table => new
                {
                    ingest_id = table.Column<int>(type: "integer", nullable: false),
                    schedule_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ingest_schedule", x => new { x.ingest_id, x.schedule_id });
                    table.ForeignKey(
                        name: "FK_ingest_schedule_ingest_ingest_id",
                        column: x => x.ingest_id,
                        principalTable: "ingest",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ingest_schedule_schedule_schedule_id",
                        column: x => x.schedule_id,
                        principalTable: "schedule",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ingest_state",
                columns: table => new
                {
                    ingest_id = table.Column<int>(type: "integer", nullable: false),
                    last_ran_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    creation_date_of_last_item = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failed_attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ingest_state", x => x.ingest_id);
                    table.ForeignKey(
                        name: "FK_ingest_state_ingest_ingest_id",
                        column: x => x.ingest_id,
                        principalTable: "ingest",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "av_overview_section",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    av_overview_instance_id = table.Column<long>(type: "bigint", nullable: false),
                    source_id = table.Column<int>(type: "integer", nullable: true),
                    other_source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    series_id = table.Column<int>(type: "integer", nullable: true),
                    anchors = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    start_time = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_av_overview_section", x => x.id);
                    table.ForeignKey(
                        name: "FK_av_overview_section_av_overview_instance_av_overview_instan~",
                        column: x => x.av_overview_instance_id,
                        principalTable: "av_overview_instance",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_av_overview_section_series_series_id",
                        column: x => x.series_id,
                        principalTable: "series",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_av_overview_section_source_source_id",
                        column: x => x.source_id,
                        principalTable: "source",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "av_overview_template_section",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    av_overview_template_id = table.Column<int>(type: "integer", nullable: false),
                    source_id = table.Column<int>(type: "integer", nullable: true),
                    other_source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    series_id = table.Column<int>(type: "integer", nullable: true),
                    anchors = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    start_time = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_av_overview_template_section", x => x.id);
                    table.ForeignKey(
                        name: "FK_av_overview_template_section_av_overview_template_av_overvi~",
                        column: x => x.av_overview_template_id,
                        principalTable: "av_overview_template",
                        principalColumn: "template_type",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_av_overview_template_section_series_series_id",
                        column: x => x.series_id,
                        principalTable: "series",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_av_overview_template_section_source_source_id",
                        column: x => x.source_id,
                        principalTable: "source",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    status = table.Column<int>(type: "integer", nullable: false),
                    content_type = table.Column<int>(type: "integer", nullable: false),
                    source_id = table.Column<int>(type: "integer", nullable: true),
                    source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    license_id = table.Column<int>(type: "integer", nullable: false),
                    media_type_id = table.Column<int>(type: "integer", nullable: false),
                    series_id = table.Column<int>(type: "integer", nullable: true),
                    contributor_id = table.Column<int>(type: "integer", nullable: true),
                    owner_id = table.Column<int>(type: "integer", nullable: true),
                    headline = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    byline = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    uid = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    external_uid = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, defaultValue: ""),
                    edition = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    section = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    page = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    published_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    posted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    summary = table.Column<string>(type: "text", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    source_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false),
                    is_approved = table.Column<bool>(type: "boolean", nullable: false),
                    is_private = table.Column<bool>(type: "boolean", nullable: false),
                    versions = table.Column<Dictionary<int, ContentVersion>>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    IngestTypeId = table.Column<int>(type: "integer", nullable: true),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content", x => x.id);
                    table.ForeignKey(
                        name: "FK_content_contributor_contributor_id",
                        column: x => x.contributor_id,
                        principalTable: "contributor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_content_ingest_type_IngestTypeId",
                        column: x => x.IngestTypeId,
                        principalTable: "ingest_type",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_content_license_license_id",
                        column: x => x.license_id,
                        principalTable: "license",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_content_media_type_media_type_id",
                        column: x => x.media_type_id,
                        principalTable: "media_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_content_series_series_id",
                        column: x => x.series_id,
                        principalTable: "series",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_content_source_source_id",
                        column: x => x.source_id,
                        principalTable: "source",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_content_user_owner_id",
                        column: x => x.owner_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "series_media_type_search_mapping",
                columns: table => new
                {
                    series_id = table.Column<int>(type: "integer", nullable: false),
                    media_type_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_series_media_type_search_mapping", x => new { x.series_id, x.media_type_id });
                    table.ForeignKey(
                        name: "FK_series_media_type_search_mapping_media_type_media_type_id",
                        column: x => x.media_type_id,
                        principalTable: "media_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_series_media_type_search_mapping_series_series_id",
                        column: x => x.series_id,
                        principalTable: "series",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "topic_score_rule",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source_id = table.Column<int>(type: "integer", nullable: false),
                    series_id = table.Column<int>(type: "integer", nullable: true),
                    section = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    page_min = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    page_max = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    has_image = table.Column<bool>(type: "boolean", nullable: true),
                    time_min = table.Column<TimeSpan>(type: "interval", nullable: true),
                    time_max = table.Column<TimeSpan>(type: "interval", nullable: true),
                    char_min = table.Column<int>(type: "integer", nullable: true),
                    char_max = table.Column<int>(type: "integer", nullable: true),
                    score = table.Column<int>(type: "integer", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_topic_score_rule", x => x.id);
                    table.ForeignKey(
                        name: "FK_topic_score_rule_series_series_id",
                        column: x => x.series_id,
                        principalTable: "series",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_topic_score_rule_source_source_id",
                        column: x => x.source_id,
                        principalTable: "source",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_section_chart_template",
                columns: table => new
                {
                    report_section_id = table.Column<int>(type: "integer", nullable: false),
                    chart_template_id = table.Column<int>(type: "integer", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    settings = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_section_chart_template", x => new { x.report_section_id, x.chart_template_id });
                    table.ForeignKey(
                        name: "FK_report_section_chart_template_chart_template_chart_template~",
                        column: x => x.chart_template_id,
                        principalTable: "chart_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_section_chart_template_report_section_report_section~",
                        column: x => x.report_section_id,
                        principalTable: "report_section",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "av_overview_template_section_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    av_overview_template_section_id = table.Column<int>(type: "integer", nullable: false),
                    item_type = table.Column<int>(type: "integer", nullable: false),
                    time = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_av_overview_template_section_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_av_overview_template_section_item_av_overview_template_sect~",
                        column: x => x.av_overview_template_section_id,
                        principalTable: "av_overview_template_section",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "av_overview_section_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    av_overview_section_id = table.Column<int>(type: "integer", nullable: false),
                    item_type = table.Column<int>(type: "integer", nullable: false),
                    time = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    content_id = table.Column<long>(type: "bigint", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_av_overview_section_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_av_overview_section_item_av_overview_section_av_overview_se~",
                        column: x => x.av_overview_section_id,
                        principalTable: "av_overview_section",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_av_overview_section_item_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_action",
                columns: table => new
                {
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    action_id = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_action", x => new { x.content_id, x.action_id });
                    table.ForeignKey(
                        name: "FK_content_action_action_action_id",
                        column: x => x.action_id,
                        principalTable: "action",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_content_action_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_label",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    value = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_label", x => x.id);
                    table.ForeignKey(
                        name: "FK_content_label_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_link",
                columns: table => new
                {
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    link_id = table.Column<long>(type: "bigint", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_link", x => new { x.content_id, x.link_id });
                    table.ForeignKey(
                        name: "FK_content_link_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_content_link_content_link_id",
                        column: x => x.link_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_log",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_log", x => x.id);
                    table.ForeignKey(
                        name: "FK_content_log_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_tag",
                columns: table => new
                {
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    tag_id = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_tag", x => new { x.content_id, x.tag_id });
                    table.ForeignKey(
                        name: "FK_content_tag_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_content_tag_tag_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tag",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_tone",
                columns: table => new
                {
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    tone_pool_id = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_tone", x => new { x.content_id, x.tone_pool_id });
                    table.ForeignKey(
                        name: "FK_content_tone_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_content_tone_tone_pool_tone_pool_id",
                        column: x => x.tone_pool_id,
                        principalTable: "tone_pool",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_topic",
                columns: table => new
                {
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    topic_id = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_topic", x => new { x.content_id, x.topic_id });
                    table.ForeignKey(
                        name: "FK_content_topic_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_content_topic_topic_topic_id",
                        column: x => x.topic_id,
                        principalTable: "topic",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "file_reference",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    file_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    size = table.Column<long>(type: "bigint", nullable: false),
                    running_time = table.Column<long>(type: "bigint", nullable: false),
                    is_uploaded = table.Column<bool>(type: "boolean", nullable: false),
                    is_synced_to_s3 = table.Column<bool>(type: "boolean", nullable: false),
                    s3_path = table.Column<string>(type: "text", nullable: true),
                    last_synced_to_s3_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_file_reference", x => x.id);
                    table.ForeignKey(
                        name: "FK_file_reference_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "folder_content",
                columns: table => new
                {
                    folder_id = table.Column<int>(type: "integer", nullable: false),
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_folder_content", x => new { x.folder_id, x.content_id });
                    table.ForeignKey(
                        name: "FK_folder_content_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_folder_content_folder_folder_id",
                        column: x => x.folder_id,
                        principalTable: "folder",
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
                    owner_id = table.Column<int>(type: "integer", nullable: true),
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    sent_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    response = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    subject = table.Column<string>(type: "text", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
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
                name: "quote",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    statement = table.Column<string>(type: "text", nullable: false),
                    byline = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_relevant = table.Column<bool>(type: "boolean", nullable: false),
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quote", x => x.id);
                    table.ForeignKey(
                        name: "FK_quote_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report_instance_content",
                columns: table => new
                {
                    report_instance_id = table.Column<long>(type: "bigint", nullable: false),
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    section_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValueSql: "''"),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_instance_content", x => new { x.report_instance_id, x.content_id, x.section_name });
                    table.ForeignKey(
                        name: "FK_report_instance_content_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_instance_content_report_instance_report_instance_id",
                        column: x => x.report_instance_id,
                        principalTable: "report_instance",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "time_tracking",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    effort = table.Column<float>(type: "real", nullable: false),
                    activity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_time_tracking", x => x.id);
                    table.ForeignKey(
                        name: "FK_time_tracking_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_time_tracking_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_content_notification",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    content_id = table.Column<long>(type: "bigint", nullable: false),
                    is_subscribed = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_content_notification", x => new { x.user_id, x.content_id });
                    table.ForeignKey(
                        name: "FK_user_content_notification_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_content_notification_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "work_order",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    work_type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    requestor_id = table.Column<int>(type: "integer", nullable: true),
                    assigned_id = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    note = table.Column<string>(type: "text", nullable: false),
                    configuration = table.Column<JsonDocument>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    content_id = table.Column<long>(type: "bigint", nullable: true),
                    created_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_by = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    updated_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_order", x => x.id);
                    table.ForeignKey(
                        name: "FK_work_order_content_content_id",
                        column: x => x.content_id,
                        principalTable: "content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_work_order_user_assigned_id",
                        column: x => x.assigned_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_work_order_user_requestor_id",
                        column: x => x.requestor_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_action",
                table: "action",
                columns: new[] { "value_type", "value_label" });

            migrationBuilder.CreateIndex(
                name: "IX_action_is_enabled",
                table: "action",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_name",
                table: "action",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_instance_published_on",
                table: "av_overview_instance",
                column: "published_on",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_instance_template_type_published_on",
                table: "av_overview_instance",
                columns: new[] { "template_type", "published_on" });

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_section_av_overview_instance_id",
                table: "av_overview_section",
                column: "av_overview_instance_id");

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_section_series_id",
                table: "av_overview_section",
                column: "series_id");

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_section_source_id",
                table: "av_overview_section",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_section_item_av_overview_section_id",
                table: "av_overview_section_item",
                column: "av_overview_section_id");

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_section_item_content_id",
                table: "av_overview_section_item",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_template_report_template_id",
                table: "av_overview_template",
                column: "report_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_template_section_av_overview_template_id",
                table: "av_overview_template_section",
                column: "av_overview_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_template_section_series_id",
                table: "av_overview_template_section",
                column: "series_id");

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_template_section_source_id",
                table: "av_overview_template_section",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_av_overview_template_section_item_av_overview_template_sect~",
                table: "av_overview_template_section_item",
                column: "av_overview_template_section_id");

            migrationBuilder.CreateIndex(
                name: "IX_cache",
                table: "cache",
                columns: new[] { "key", "value" });

            migrationBuilder.CreateIndex(
                name: "IX_chart_template_name",
                table: "chart_template",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_charttemplate_is_enabled",
                table: "chart_template",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_connection_is_enabled",
                table: "connection",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_name1",
                table: "connection",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_content",
                table: "content",
                columns: new[] { "content_type", "source", "uid", "page", "status", "is_hidden" });

            migrationBuilder.CreateIndex(
                name: "IX_content_contributor_id",
                table: "content",
                column: "contributor_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_dates",
                table: "content",
                columns: new[] { "published_on", "created_on" });

            migrationBuilder.CreateIndex(
                name: "IX_content_IngestTypeId",
                table: "content",
                column: "IngestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_content_license_id",
                table: "content",
                column: "license_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_media_type_id",
                table: "content",
                column: "media_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_owner_id",
                table: "content",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_series_id",
                table: "content",
                column: "series_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_source_id",
                table: "content",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_headline",
                table: "content",
                column: "headline");

            migrationBuilder.CreateIndex(
                name: "IX_print_content",
                table: "content",
                columns: new[] { "edition", "section", "byline" });

            migrationBuilder.CreateIndex(
                name: "IX_content_action_action_id",
                table: "content_action",
                column: "action_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_label",
                table: "content_label",
                columns: new[] { "key", "value" });

            migrationBuilder.CreateIndex(
                name: "IX_content_label_content_id",
                table: "content_label",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_link_link_id",
                table: "content_link",
                column: "link_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_log_content_id",
                table: "content_log",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_reference",
                table: "content_reference",
                columns: new[] { "published_on", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_source_uid",
                table: "content_reference",
                columns: new[] { "source", "uid" });

            migrationBuilder.CreateIndex(
                name: "IX_content_tag_tag_id",
                table: "content_tag",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_tone_tone_pool_id",
                table: "content_tone",
                column: "tone_pool_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_topic_topic_id",
                table: "content_topic",
                column: "topic_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_type_action_action_id",
                table: "content_type_action",
                column: "action_id");

            migrationBuilder.CreateIndex(
                name: "IX_contributor_is_enabled",
                table: "contributor",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_contributor_source_id",
                table: "contributor",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_data_location_connection_id",
                table: "data_location",
                column: "connection_id");

            migrationBuilder.CreateIndex(
                name: "IX_datalocation_is_enabled",
                table: "data_location",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_name2",
                table: "data_location",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_event_schedule_folder_id",
                table: "event_schedule",
                column: "folder_id");

            migrationBuilder.CreateIndex(
                name: "IX_event_schedule_notification_id",
                table: "event_schedule",
                column: "notification_id");

            migrationBuilder.CreateIndex(
                name: "IX_event_schedule_report_id",
                table: "event_schedule",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_event_schedule_schedule_id",
                table: "event_schedule",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "IX_file_reference_content_id",
                table: "file_reference",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_filter_is_enabled",
                table: "filter",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_filter_owner_id_name",
                table: "filter",
                columns: new[] { "owner_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_folder_filter_id",
                table: "folder",
                column: "filter_id");

            migrationBuilder.CreateIndex(
                name: "IX_folder_is_enabled",
                table: "folder",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_folder_owner_id_name",
                table: "folder",
                columns: new[] { "owner_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_folder_content_content_id",
                table: "folder_content",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_ingest",
                table: "ingest",
                columns: new[] { "ingest_type_id", "source_id", "topic" });

            migrationBuilder.CreateIndex(
                name: "IX_ingest_destination_connection_id",
                table: "ingest",
                column: "destination_connection_id");

            migrationBuilder.CreateIndex(
                name: "IX_ingest_media_type_id",
                table: "ingest",
                column: "media_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_ingest_source_connection_id",
                table: "ingest",
                column: "source_connection_id");

            migrationBuilder.CreateIndex(
                name: "IX_ingest_source_id",
                table: "ingest",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_name3",
                table: "ingest",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ingest_data_location_data_location_id",
                table: "ingest_data_location",
                column: "data_location_id");

            migrationBuilder.CreateIndex(
                name: "IX_ingest_schedule_schedule_id",
                table: "ingest_schedule",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "IX_ingesttype_is_enabled",
                table: "ingest_type",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_name4",
                table: "ingest_type",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_license_is_enabled",
                table: "license",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_name5",
                table: "license",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mediatype_is_enabled",
                table: "media_type",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_name6",
                table: "media_type",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_metric_is_enabled",
                table: "metric",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_name7",
                table: "metric",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_minister_is_enabled",
                table: "minister",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_minister_name",
                table: "minister",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_minister_organization_id",
                table: "minister",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_is_enabled",
                table: "notification",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_notification_template_id",
                table: "notification",
                column: "notification_template_id");

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

            migrationBuilder.CreateIndex(
                name: "IX_organization_is_enabled",
                table: "organization",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_organization_parent_id_name",
                table: "organization",
                columns: new[] { "parent_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_is_enabled",
                table: "product",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_product_name_target_product_id_product_type",
                table: "product",
                columns: new[] { "name", "target_product_id", "product_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quote_content_id",
                table: "quote",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_statement",
                table: "quote",
                column: "statement");

            migrationBuilder.CreateIndex(
                name: "IX_report_is_enabled",
                table: "report",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_report_owner_id_name",
                table: "report",
                columns: new[] { "owner_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_report_report_template_id",
                table: "report",
                column: "report_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_dates",
                table: "report_instance",
                columns: new[] { "published_on", "created_on" });

            migrationBuilder.CreateIndex(
                name: "IX_report_instance_owner_id",
                table: "report_instance",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_instance_report_id",
                table: "report_instance",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_instance_content_content_id",
                table: "report_instance_content",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_section_filter_id",
                table: "report_section",
                column: "filter_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_section_folder_id",
                table: "report_section",
                column: "folder_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_section_linked_report_id",
                table: "report_section",
                column: "linked_report_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_section_report_id_name",
                table: "report_section",
                columns: new[] { "report_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reportsection_is_enabled",
                table: "report_section",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_report_section_chart_template_chart_template_id",
                table: "report_section_chart_template",
                column: "chart_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_template_is_public_is_enabled",
                table: "report_template",
                columns: new[] { "is_public", "is_enabled" });

            migrationBuilder.CreateIndex(
                name: "IX_report_template_name",
                table: "report_template",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reporttemplate_is_enabled",
                table: "report_template",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_report_template_chart_template_chart_template_id",
                table: "report_template_chart_template",
                column: "chart_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_schedule",
                table: "schedule",
                columns: new[] { "name", "is_enabled" });

            migrationBuilder.CreateIndex(
                name: "IX_schedule_requested_by_id",
                table: "schedule",
                column: "requested_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_sentiment_is_enabled",
                table: "sentiment",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_sentiment_name",
                table: "sentiment",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_series_is_enabled",
                table: "series",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_series_source_id",
                table: "series",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_series_media_type_search_mapping_media_type_id",
                table: "series_media_type_search_mapping",
                column: "media_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_setting_is_enabled",
                table: "setting",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_setting_name",
                table: "setting",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_source_code",
                table: "source",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_source_is_enabled",
                table: "source",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_source_license_id",
                table: "source",
                column: "license_id");

            migrationBuilder.CreateIndex(
                name: "IX_source_media_type_id",
                table: "source",
                column: "media_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_source_name",
                table: "source",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_source_owner_id",
                table: "source",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_source_media_type_search_mapping_media_type_id",
                table: "source_media_type_search_mapping",
                column: "media_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_source_metric_metric_id",
                table: "source_metric",
                column: "metric_id");

            migrationBuilder.CreateIndex(
                name: "IX_tag_code",
                table: "tag",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tag_is_enabled",
                table: "tag",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_tag_name",
                table: "tag",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_time_tracking_content_id",
                table: "time_tracking",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_time_tracking_user_id",
                table: "time_tracking",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tone_pool_name",
                table: "tone_pool",
                columns: new[] { "owner_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tonepool_is_enabled",
                table: "tone_pool",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_topic_is_enabled",
                table: "topic",
                columns: new[] { "is_enabled", "name" });

            migrationBuilder.CreateIndex(
                name: "IX_topic_name",
                table: "topic",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_source_id_series_id_section",
                table: "topic_score_rule",
                columns: new[] { "source_id", "series_id", "section" });

            migrationBuilder.CreateIndex(
                name: "IX_topic_score_rule_series_id",
                table: "topic_score_rule",
                column: "series_id");

            migrationBuilder.CreateIndex(
                name: "IX_email",
                table: "user",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_key",
                table: "user",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_last_first_name",
                table: "user",
                columns: new[] { "last_name", "first_name" });

            migrationBuilder.CreateIndex(
                name: "IX_username",
                table: "user",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_av_overview_av_overview_template_type",
                table: "user_av_overview",
                column: "av_overview_template_type");

            migrationBuilder.CreateIndex(
                name: "IX_user_av_overview_instance_report_instance_id",
                table: "user_av_overview_instance",
                column: "report_instance_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_colleague_colleague_id",
                table: "user_colleague",
                column: "colleague_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_content_notification_content_id",
                table: "user_content_notification",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_distribution_linked_user_id",
                table: "user_distribution",
                column: "linked_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_media_type_media_type_id",
                table: "user_media_type",
                column: "media_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_notification_notification_id",
                table: "user_notification",
                column: "notification_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_organization_organization_id",
                table: "user_organization",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_product_product_id",
                table: "user_product",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_report_report_id",
                table: "user_report",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_report_instance_report_instance_id",
                table: "user_report_instance",
                column: "report_instance_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_source_source_id",
                table: "user_source",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_order",
                table: "work_order",
                columns: new[] { "work_type", "status", "created_on", "requestor_id", "assigned_id" });

            migrationBuilder.CreateIndex(
                name: "IX_work_order_assigned_id",
                table: "work_order",
                column: "assigned_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_content_id",
                table: "work_order",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_requestor_id",
                table: "work_order",
                column: "requestor_id");
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropTable(
                name: "av_overview_section_item");

            migrationBuilder.DropTable(
                name: "av_overview_template_section_item");

            migrationBuilder.DropTable(
                name: "cache");

            migrationBuilder.DropTable(
                name: "content_action");

            migrationBuilder.DropTable(
                name: "content_label");

            migrationBuilder.DropTable(
                name: "content_link");

            migrationBuilder.DropTable(
                name: "content_log");

            migrationBuilder.DropTable(
                name: "content_reference");

            migrationBuilder.DropTable(
                name: "content_tag");

            migrationBuilder.DropTable(
                name: "content_tone");

            migrationBuilder.DropTable(
                name: "content_topic");

            migrationBuilder.DropTable(
                name: "content_type_action");

            migrationBuilder.DropTable(
                name: "earned_media");

            migrationBuilder.DropTable(
                name: "event_schedule");

            migrationBuilder.DropTable(
                name: "file_reference");

            migrationBuilder.DropTable(
                name: "folder_content");

            migrationBuilder.DropTable(
                name: "ingest_data_location");

            migrationBuilder.DropTable(
                name: "ingest_schedule");

            migrationBuilder.DropTable(
                name: "ingest_state");

            migrationBuilder.DropTable(
                name: "minister");

            migrationBuilder.DropTable(
                name: "notification_instance");

            migrationBuilder.DropTable(
                name: "quote");

            migrationBuilder.DropTable(
                name: "report_instance_content");

            migrationBuilder.DropTable(
                name: "report_section_chart_template");

            migrationBuilder.DropTable(
                name: "report_template_chart_template");

            migrationBuilder.DropTable(
                name: "sentiment");

            migrationBuilder.DropTable(
                name: "series_media_type_search_mapping");

            migrationBuilder.DropTable(
                name: "setting");

            migrationBuilder.DropTable(
                name: "source_media_type_search_mapping");

            migrationBuilder.DropTable(
                name: "source_metric");

            migrationBuilder.DropTable(
                name: "system_message");

            migrationBuilder.DropTable(
                name: "time_tracking");

            migrationBuilder.DropTable(
                name: "topic_score_rule");

            migrationBuilder.DropTable(
                name: "user_av_overview");

            migrationBuilder.DropTable(
                name: "user_av_overview_instance");

            migrationBuilder.DropTable(
                name: "user_colleague");

            migrationBuilder.DropTable(
                name: "user_content_notification");

            migrationBuilder.DropTable(
                name: "user_distribution");

            migrationBuilder.DropTable(
                name: "user_media_type");

            migrationBuilder.DropTable(
                name: "user_notification");

            migrationBuilder.DropTable(
                name: "user_organization");

            migrationBuilder.DropTable(
                name: "user_product");

            migrationBuilder.DropTable(
                name: "user_report");

            migrationBuilder.DropTable(
                name: "user_report_instance");

            migrationBuilder.DropTable(
                name: "user_source");

            migrationBuilder.DropTable(
                name: "work_order");

            migrationBuilder.DropTable(
                name: "av_overview_section");

            migrationBuilder.DropTable(
                name: "av_overview_template_section");

            migrationBuilder.DropTable(
                name: "tag");

            migrationBuilder.DropTable(
                name: "tone_pool");

            migrationBuilder.DropTable(
                name: "topic");

            migrationBuilder.DropTable(
                name: "action");

            migrationBuilder.DropTable(
                name: "data_location");

            migrationBuilder.DropTable(
                name: "schedule");

            migrationBuilder.DropTable(
                name: "ingest");

            migrationBuilder.DropTable(
                name: "report_section");

            migrationBuilder.DropTable(
                name: "chart_template");

            migrationBuilder.DropTable(
                name: "metric");

            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropTable(
                name: "organization");

            migrationBuilder.DropTable(
                name: "product");

            migrationBuilder.DropTable(
                name: "report_instance");

            migrationBuilder.DropTable(
                name: "content");

            migrationBuilder.DropTable(
                name: "av_overview_instance");

            migrationBuilder.DropTable(
                name: "connection");

            migrationBuilder.DropTable(
                name: "folder");

            migrationBuilder.DropTable(
                name: "notification_template");

            migrationBuilder.DropTable(
                name: "report");

            migrationBuilder.DropTable(
                name: "contributor");

            migrationBuilder.DropTable(
                name: "ingest_type");

            migrationBuilder.DropTable(
                name: "series");

            migrationBuilder.DropTable(
                name: "av_overview_template");

            migrationBuilder.DropTable(
                name: "filter");

            migrationBuilder.DropTable(
                name: "source");

            migrationBuilder.DropTable(
                name: "report_template");

            migrationBuilder.DropTable(
                name: "license");

            migrationBuilder.DropTable(
                name: "media_type");

            migrationBuilder.DropTable(
                name: "user");
            PostDown(migrationBuilder);
        }
    }
}
