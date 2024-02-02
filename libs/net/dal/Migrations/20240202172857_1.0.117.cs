using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _10117 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);

            migrationBuilder.DropForeignKey(
                name: "FK_ingest_service_ingest_ingest_id",
                table: "ingest_service");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ingest_service",
                table: "ingest_service");

            migrationBuilder.RenameTable(
                name:"ingest_service",
                newName: "ingest_state");

            migrationBuilder.AddColumn<string>(
                name: "creation_date_of_last_item",
                table: "ingest_state",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ingest_state",
                table: "ingest_state",
                column: "ingest_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ingest_state_ingest_ingest_id",
                table: "ingest_state",
                column: "ingest_id",
                principalTable: "ingest",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);

            migrationBuilder.DropForeignKey(
                name: "FK_ingest_state_ingest_ingest_id",
                table: "ingest_state");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ingest_state",
                table: "ingest_state");

            migrationBuilder.DropColumn(
                name: "creation_date_of_last_item",
                table: "ingest_state");

            migrationBuilder.RenameTable(
                name:"ingest_state",
                newName: "ingest_service");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ingest_service",
                table: "ingest_service",
                column: "ingest_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ingest_service_ingest_ingest_id",
                table: "ingest_service",
                column: "ingest_id",
                principalTable: "ingest",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            PostDown(migrationBuilder);
        }
    }
}
