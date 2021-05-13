using Microsoft.EntityFrameworkCore.Migrations;

namespace diet_tracker_api.Migrations
{
    public partial class refactoruserdailytrackingkeyswithoccurance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDailyTracking",
                table: "UserDailyTracking");

            migrationBuilder.RenameColumn(
                name: "Occurance",
                table: "UserTracking",
                newName: "Occurances");

            migrationBuilder.AddColumn<int>(
                name: "Occurance",
                table: "UserDailyTracking",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDailyTracking",
                table: "UserDailyTracking",
                columns: new[] { "UserId", "Day", "UserTrackingId", "Occurance" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDailyTracking",
                table: "UserDailyTracking");

            migrationBuilder.DropColumn(
                name: "Occurance",
                table: "UserDailyTracking");

            migrationBuilder.RenameColumn(
                name: "Occurances",
                table: "UserTracking",
                newName: "Occurance");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDailyTracking",
                table: "UserDailyTracking",
                columns: new[] { "UserId", "Day", "UserTrackingId" });
        }
    }
}
