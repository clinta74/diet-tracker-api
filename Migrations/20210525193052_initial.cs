using Microsoft.EntityFrameworkCore.Migrations;

namespace diet_tracker_api.Migrations
{
    
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fueling",
                columns: table => new
                {
                    FuelingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fueling", x => x.FuelingId);
                });

            migrationBuilder.CreateTable(
                name: "Plan",
                columns: table => new
                {
                    PlanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FuelingCount = table.Column<int>(type: "int", nullable: false),
                    MealCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plan", x => x.PlanId);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WaterTarget = table.Column<int>(type: "int", nullable: false, defaultValue: 64),
                    WaterSize = table.Column<int>(type: "int", nullable: false, defaultValue: 8)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "UserDay",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    Day = table.Column<DateTime>(type: "date", nullable: false),
                    Water = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Condiments = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDay", x => new { x.UserId, x.Day });
                    table.ForeignKey(
                        name: "FK_UserDay_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPlan",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    Start = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPlan", x => new { x.UserId, x.PlanId, x.Start });
                    table.ForeignKey(
                        name: "FK_UserPlan_Plan_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plan",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPlan_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTracking",
                columns: table => new
                {
                    UserTrackingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Occurrences = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Victory",
                columns: table => new
                {
                    VictoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    When = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Victory", x => x.VictoryId);
                    table.ForeignKey(
                        name: "FK_Victory_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserFueling",
                columns: table => new
                {
                    UserFuelingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    Day = table.Column<DateTime>(type: "date", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    When = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFueling", x => x.UserFuelingId);
                    table.ForeignKey(
                        name: "FK_UserFueling_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserFueling_UserDay_UserId_Day",
                        columns: x => new { x.UserId, x.Day },
                        principalTable: "UserDay",
                        principalColumns: new[] { "UserId", "Day" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserMeal",
                columns: table => new
                {
                    UserMealId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    Day = table.Column<DateTime>(type: "date", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    When = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMeal", x => x.UserMealId);
                    table.ForeignKey(
                        name: "FK_UserMeal_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserMeal_UserDay_UserId_Day",
                        columns: x => new { x.UserId, x.Day },
                        principalTable: "UserDay",
                        principalColumns: new[] { "UserId", "Day" },
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "UserTrackingValue",
                columns: table => new
                {
                    UserTrackingValueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserTrackingId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Removed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTrackingValue", x => x.UserTrackingValueId);
                    table.ForeignKey(
                        name: "FK_UserTrackingValue_UserTracking_UserTrackingId",
                        column: x => x.UserTrackingId,
                        principalTable: "UserTracking",
                        principalColumn: "UserTrackingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDailyTrackingValue",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    Day = table.Column<DateTime>(type: "date", nullable: false),
                    UserTrackingId = table.Column<int>(type: "int", nullable: false),
                    Occurrence = table.Column<int>(type: "int", nullable: false),
                    UserTrackingValueId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    When = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDailyTrackingValue", x => new { x.UserId, x.Day, x.UserTrackingId, x.Occurrence, x.UserTrackingValueId });
                    table.ForeignKey(
                        name: "FK_UserDailyTrackingValue_UserDailyTracking_UserId_Day_UserTrackingId_Occurrence",
                        columns: x => new { x.UserId, x.Day, x.UserTrackingId, x.Occurrence },
                        principalTable: "UserDailyTracking",
                        principalColumns: new[] { "UserId", "Day", "UserTrackingId", "Occurrence" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDailyTrackingValue_UserTrackingValue_UserTrackingValueId",
                        column: x => x.UserTrackingValueId,
                        principalTable: "UserTrackingValue",
                        principalColumn: "UserTrackingValueId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDailyTracking_UserTrackingId",
                table: "UserDailyTracking",
                column: "UserTrackingId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDailyTrackingValue_UserTrackingValueId",
                table: "UserDailyTrackingValue",
                column: "UserTrackingValueId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFueling_UserId_Day",
                table: "UserFueling",
                columns: new[] { "UserId", "Day" });

            migrationBuilder.CreateIndex(
                name: "IX_UserMeal_UserId_Day",
                table: "UserMeal",
                columns: new[] { "UserId", "Day" });

            migrationBuilder.CreateIndex(
                name: "IX_UserPlan_PlanId",
                table: "UserPlan",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTracking_UserId",
                table: "UserTracking",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTrackingValue_UserTrackingId",
                table: "UserTrackingValue",
                column: "UserTrackingId");

            migrationBuilder.CreateIndex(
                name: "IX_Victory_UserId",
                table: "Victory",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fueling");

            migrationBuilder.DropTable(
                name: "UserDailyTrackingValue");

            migrationBuilder.DropTable(
                name: "UserFueling");

            migrationBuilder.DropTable(
                name: "UserMeal");

            migrationBuilder.DropTable(
                name: "UserPlan");

            migrationBuilder.DropTable(
                name: "Victory");

            migrationBuilder.DropTable(
                name: "UserDailyTracking");

            migrationBuilder.DropTable(
                name: "UserTrackingValue");

            migrationBuilder.DropTable(
                name: "Plan");

            migrationBuilder.DropTable(
                name: "UserDay");

            migrationBuilder.DropTable(
                name: "UserTracking");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
