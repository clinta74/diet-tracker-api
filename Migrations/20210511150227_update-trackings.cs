using Microsoft.EntityFrameworkCore.Migrations;

namespace diet_tracker_api.Migrations
{
    public partial class updatetrackings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "UserDailyTracking");

            migrationBuilder.AddColumn<int>(
                name: "UserTrackingId",
                table: "UserDailyTracking",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserDailyTracking_UserTrackingId",
                table: "UserDailyTracking",
                column: "UserTrackingId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserDailyTracking_UserTracking_UserTrackingId",
                table: "UserDailyTracking",
                column: "UserTrackingId",
                principalTable: "UserTracking",
                principalColumn: "UserTrackingId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserDailyTracking_UserTracking_UserTrackingId",
                table: "UserDailyTracking");

            migrationBuilder.DropIndex(
                name: "IX_UserDailyTracking_UserTrackingId",
                table: "UserDailyTracking");

            migrationBuilder.DropColumn(
                name: "UserTrackingId",
                table: "UserDailyTracking");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "UserDailyTracking",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
