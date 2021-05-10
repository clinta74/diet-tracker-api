using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace diet_tracker_api.Migrations
{
    public partial class addusertracking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WaterSize",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 8);

            migrationBuilder.AddColumn<int>(
                name: "WaterTarget",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 64);

            migrationBuilder.CreateTable(
                name: "UserDailyTracking",
                columns: table => new
                {
                    UserDailyTrackingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Day = table.Column<DateTime>(type: "date", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    When = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDailyTracking", x => x.UserDailyTrackingId);
                    table.ForeignKey(
                        name: "FK_UserDailyTracking_UserDay_UserId_Day",
                        columns: x => new { x.UserId, x.Day },
                        principalTable: "UserDay",
                        principalColumns: new[] { "UserId", "Day" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserTracking",
                columns: table => new
                {
                    UserTrackingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Occurance = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTracking", x => x.UserTrackingId);
                    table.ForeignKey(
                        name: "FK_UserTracking_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDailyTracking_UserId_Day",
                table: "UserDailyTracking",
                columns: new[] { "UserId", "Day" });

            migrationBuilder.CreateIndex(
                name: "IX_UserTracking_UserId",
                table: "UserTracking",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDailyTracking");

            migrationBuilder.DropTable(
                name: "UserTracking");

            migrationBuilder.DropColumn(
                name: "WaterSize",
                table: "User");

            migrationBuilder.DropColumn(
                name: "WaterTarget",
                table: "User");
        }
    }
}
