using Microsoft.EntityFrameworkCore.Migrations;
using TNO.DAL;

#nullable disable

namespace TNO.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _1320 : SeedMigration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            PreUp(migrationBuilder);
            migrationBuilder.CreateIndex(
                name: "IX_user_report_instance_link",
                table: "user_report_instance",
                columns: new[] { "link_sent_on", "link_status" });

            migrationBuilder.CreateIndex(
                name: "IX_user_report_instance_text",
                table: "user_report_instance",
                columns: new[] { "text_sent_on", "text_status" });

            migrationBuilder.CreateIndex(
                name: "IX_user_av_overview_instance",
                table: "user_av_overview_instance",
                columns: new[] { "sent_on", "status" });
            PostUp(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            PreDown(migrationBuilder);
            migrationBuilder.DropIndex(
                name: "IX_user_report_instance_link",
                table: "user_report_instance");

            migrationBuilder.DropIndex(
                name: "IX_user_report_instance_text",
                table: "user_report_instance");

            migrationBuilder.DropIndex(
                name: "IX_user_av_overview_instance",
                table: "user_av_overview_instance");
            PostDown(migrationBuilder);
        }
    }
}
