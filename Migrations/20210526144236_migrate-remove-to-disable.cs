using Microsoft.EntityFrameworkCore.Migrations;

namespace diet_tracker_api.Migrations
{
    public partial class Migrateremovetodisable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Removed",
                table: "UserTrackingValue",
                newName: "Disabled");

            migrationBuilder.RenameColumn(
                name: "Removed",
                table: "UserTracking",
                newName: "Disabled");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Disabled",
                table: "UserTrackingValue",
                newName: "Removed");

            migrationBuilder.RenameColumn(
                name: "Disabled",
                table: "UserTracking",
                newName: "Removed");
        }
    }
}
