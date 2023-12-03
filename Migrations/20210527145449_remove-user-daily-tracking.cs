using Microsoft.EntityFrameworkCore.Migrations;

namespace diet_tracker_api.Migrations
{
    public partial class Removeuserdailytracking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserDailyTrackingValue_UserDailyTracking_UserId_Day_UserTrackingId_Occurrence",
                table: "UserDailyTrackingValue");

            migrationBuilder.DropForeignKey(
                name: "FK_UserDailyTrackingValue_UserTrackingValue_UserTrackingValueId",
                table: "UserDailyTrackingValue");

            migrationBuilder.DropTable(
                name: "UserDailyTracking");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDailyTrackingValue",
                table: "UserDailyTrackingValue");

            migrationBuilder.DropColumn(
                name: "UserTrackingId",
                table: "UserDailyTrackingValue");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDailyTrackingValue",
                table: "UserDailyTrackingValue",
                columns: new[] { "UserId", "Day", "UserTrackingValueId", "Occurrence" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserDailyTrackingValue_UserDay_UserId_Day",
                table: "UserDailyTrackingValue",
                columns: new[] { "UserId", "Day" },
                principalTable: "UserDay",
                principalColumns: new[] { "UserId", "Day" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserDailyTrackingValue_UserTrackingValue_UserTrackingValueId",
                table: "UserDailyTrackingValue",
                column: "UserTrackingValueId",
                principalTable: "UserTrackingValue",
                principalColumn: "UserTrackingValueId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserDailyTrackingValue_UserDay_UserId_Day",
                table: "UserDailyTrackingValue");

            migrationBuilder.DropForeignKey(
                name: "FK_UserDailyTrackingValue_UserTrackingValue_UserTrackingValueId",
                table: "UserDailyTrackingValue");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDailyTrackingValue",
                table: "UserDailyTrackingValue");

            migrationBuilder.AddColumn<int>(
                name: "UserTrackingId",
                table: "UserDailyTrackingValue",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDailyTrackingValue",
                table: "UserDailyTrackingValue",
                columns: new[] { "UserId", "Day", "UserTrackingId", "Occurrence", "UserTrackingValueId" });

            migrationBuilder.CreateTable(
                name: "UserDailyTracking",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    Day = table.Column<DateTime>(type: "date", nullable: false),
                    UserTrackingId = table.Column<int>(type: "int", nullable: false),
                    Occurrence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDailyTracking", x => new { x.UserId, x.Day, x.UserTrackingId, x.Occurrence });
                    table.ForeignKey(
                        name: "FK_UserDailyTracking_UserDay_UserId_Day",
                        columns: x => new { x.UserId, x.Day },
                        principalTable: "UserDay",
                        principalColumns: new[] { "UserId", "Day" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDailyTracking_UserTracking_UserTrackingId",
                        column: x => x.UserTrackingId,
                        principalTable: "UserTracking",
                        principalColumn: "UserTrackingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDailyTracking_UserTrackingId",
                table: "UserDailyTracking",
                column: "UserTrackingId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserDailyTrackingValue_UserDailyTracking_UserId_Day_UserTrackingId_Occurrence",
                table: "UserDailyTrackingValue",
                columns: new[] { "UserId", "Day", "UserTrackingId", "Occurrence" },
                principalTable: "UserDailyTracking",
                principalColumns: new[] { "UserId", "Day", "UserTrackingId", "Occurrence" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserDailyTrackingValue_UserTrackingValue_UserTrackingValueId",
                table: "UserDailyTrackingValue",
                column: "UserTrackingValueId",
                principalTable: "UserTrackingValue",
                principalColumn: "UserTrackingValueId");
        }
    }
}
