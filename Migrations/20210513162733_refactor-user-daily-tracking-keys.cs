using Microsoft.EntityFrameworkCore.Migrations;

namespace diet_tracker_api.Migrations
{
    public partial class refactoruserdailytrackingkeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserDailyTracking_UserDay_UserId_Day",
                table: "UserDailyTracking");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDailyTracking",
                table: "UserDailyTracking");

            migrationBuilder.DropIndex(
                name: "IX_UserDailyTracking_UserId_Day",
                table: "UserDailyTracking");

            migrationBuilder.DropColumn(
                name: "UserDailyTrackingId",
                table: "UserDailyTracking");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserDailyTracking",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDailyTracking",
                table: "UserDailyTracking",
                columns: new[] { "UserId", "Day", "UserTrackingId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserDailyTracking_UserDay_UserId_Day",
                table: "UserDailyTracking",
                columns: new[] { "UserId", "Day" },
                principalTable: "UserDay",
                principalColumns: new[] { "UserId", "Day" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserDailyTracking_UserDay_UserId_Day",
                table: "UserDailyTracking");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDailyTracking",
                table: "UserDailyTracking");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserDailyTracking",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "UserDailyTrackingId",
                table: "UserDailyTracking",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDailyTracking",
                table: "UserDailyTracking",
                column: "UserDailyTrackingId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDailyTracking_UserId_Day",
                table: "UserDailyTracking",
                columns: new[] { "UserId", "Day" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserDailyTracking_UserDay_UserId_Day",
                table: "UserDailyTracking",
                columns: new[] { "UserId", "Day" },
                principalTable: "UserDay",
                principalColumns: new[] { "UserId", "Day" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
