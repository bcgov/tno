using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1043 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);

            migrationBuilder.RenameTable(
                name: "alert",
                newName: "system_message");

            migrationBuilder.DropPrimaryKey(
                name: "PK_alert",
                table: "system_message");

            migrationBuilder.AddPrimaryKey(
                name: "PK_system_message",
                table: "system_message",
                column: "id");

            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);

            migrationBuilder.RenameTable(
                name: "system_message",
                newName: "alert");

            migrationBuilder.DropPrimaryKey(
                name: "PK_system_message",
                table: "alert");

            migrationBuilder.AddPrimaryKey(
                name: "PK_alert",
                table: "alert",
                column: "id");

            PostDown(migrationBuilder);
        }
    }
}
