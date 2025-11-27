using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace diet_tracker_api.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fueling",
                columns: table => new
                {
                    FuelingId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fueling", x => x.FuelingId);
                });

            migrationBuilder.CreateTable(
                name: "Plan",
                columns: table => new
                {
                    PlanId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    FuelingCount = table.Column<int>(type: "integer", nullable: false),
                    MealCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plan", x => x.PlanId);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    EmailAddress = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WaterTarget = table.Column<int>(type: "integer", nullable: false, defaultValue: 64),
                    WaterSize = table.Column<int>(type: "integer", nullable: false, defaultValue: 8),
                    Autosave = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "UserDay",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(250)", nullable: false),
                    Day = table.Column<DateTime>(type: "date", nullable: false),
                    Water = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
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
                    UserId = table.Column<string>(type: "character varying(250)", nullable: false),
                    PlanId = table.Column<int>(type: "integer", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    UserTrackingId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(250)", nullable: true),
                    Disabled = table.Column<bool>(type: "boolean", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Occurrences = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    UseTime = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTracking", x => x.UserTrackingId);
                    table.ForeignKey(
                        name: "FK_UserTracking_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Victory",
                columns: table => new
                {
                    VictoryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(250)", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    When = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Victory", x => x.VictoryId);
                    table.ForeignKey(
                        name: "FK_Victory_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserFueling",
                columns: table => new
                {
                    UserFuelingId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(250)", nullable: true),
                    Day = table.Column<DateTime>(type: "date", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    When = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFueling", x => x.UserFuelingId);
                    table.ForeignKey(
                        name: "FK_UserFueling_UserDay_UserId_Day",
                        columns: x => new { x.UserId, x.Day },
                        principalTable: "UserDay",
                        principalColumns: new[] { "UserId", "Day" });
                    table.ForeignKey(
                        name: "FK_UserFueling_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserMeal",
                columns: table => new
                {
                    UserMealId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(250)", nullable: true),
                    Day = table.Column<DateTime>(type: "date", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    When = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMeal", x => x.UserMealId);
                    table.ForeignKey(
                        name: "FK_UserMeal_UserDay_UserId_Day",
                        columns: x => new { x.UserId, x.Day },
                        principalTable: "UserDay",
                        principalColumns: new[] { "UserId", "Day" });
                    table.ForeignKey(
                        name: "FK_UserMeal_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserTrackingValue",
                columns: table => new
                {
                    UserTrackingValueId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserTrackingId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Disabled = table.Column<bool>(type: "boolean", nullable: false)
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
                    UserId = table.Column<string>(type: "character varying(250)", nullable: false),
                    Day = table.Column<DateTime>(type: "date", nullable: false),
                    Occurrence = table.Column<int>(type: "integer", nullable: false),
                    UserTrackingValueId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    When = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDailyTrackingValue", x => new { x.UserId, x.Day, x.UserTrackingValueId, x.Occurrence });
                    table.ForeignKey(
                        name: "FK_UserDailyTrackingValue_UserDay_UserId_Day",
                        columns: x => new { x.UserId, x.Day },
                        principalTable: "UserDay",
                        principalColumns: new[] { "UserId", "Day" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDailyTrackingValue_UserTrackingValue_UserTrackingValueId",
                        column: x => x.UserTrackingValueId,
                        principalTable: "UserTrackingValue",
                        principalColumn: "UserTrackingValueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTrackingValueMetadata",
                columns: table => new
                {
                    UserTrackingValueId = table.Column<int>(type: "integer", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTrackingValueMetadata", x => new { x.UserTrackingValueId, x.Key });
                    table.ForeignKey(
                        name: "FK_UserTrackingValueMetadata_UserTrackingValue_UserTrackingVal~",
                        column: x => x.UserTrackingValueId,
                        principalTable: "UserTrackingValue",
                        principalColumn: "UserTrackingValueId",
                        onDelete: ReferentialAction.Cascade);
                });

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

        /// <inheritdoc />
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
                name: "UserTrackingValueMetadata");

            migrationBuilder.DropTable(
                name: "Victory");

            migrationBuilder.DropTable(
                name: "UserDay");

            migrationBuilder.DropTable(
                name: "Plan");

            migrationBuilder.DropTable(
                name: "UserTrackingValue");

            migrationBuilder.DropTable(
                name: "UserTracking");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
