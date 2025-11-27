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
                name: "fuelings",
                columns: table => new
                {
                    FuelingId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fuelings", x => x.FuelingId);
                });

            migrationBuilder.CreateTable(
                name: "plans",
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
                    table.PrimaryKey("PK_plans", x => x.PlanId);
                });

            migrationBuilder.CreateTable(
                name: "users",
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
                    table.PrimaryKey("PK_users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "user_days",
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
                    table.PrimaryKey("PK_user_days", x => new { x.UserId, x.Day });
                    table.ForeignKey(
                        name: "FK_user_days_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_plans",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(250)", nullable: false),
                    PlanId = table.Column<int>(type: "integer", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_plans", x => new { x.UserId, x.PlanId, x.Start });
                    table.ForeignKey(
                        name: "FK_user_plans_plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "plans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_plans_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_trackings",
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
                    table.PrimaryKey("PK_user_trackings", x => x.UserTrackingId);
                    table.ForeignKey(
                        name: "FK_user_trackings_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "victories",
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
                    table.PrimaryKey("PK_victories", x => x.VictoryId);
                    table.ForeignKey(
                        name: "FK_victories_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "user_fuelings",
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
                    table.PrimaryKey("PK_user_fuelings", x => x.UserFuelingId);
                    table.ForeignKey(
                        name: "FK_user_fuelings_user_days_UserId_Day",
                        columns: x => new { x.UserId, x.Day },
                        principalTable: "user_days",
                        principalColumns: new[] { "UserId", "Day" });
                    table.ForeignKey(
                        name: "FK_user_fuelings_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "user_meals",
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
                    table.PrimaryKey("PK_user_meals", x => x.UserMealId);
                    table.ForeignKey(
                        name: "FK_user_meals_user_days_UserId_Day",
                        columns: x => new { x.UserId, x.Day },
                        principalTable: "user_days",
                        principalColumns: new[] { "UserId", "Day" });
                    table.ForeignKey(
                        name: "FK_user_meals_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "user_tracking_values",
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
                    table.PrimaryKey("PK_user_tracking_values", x => x.UserTrackingValueId);
                    table.ForeignKey(
                        name: "FK_user_tracking_values_user_trackings_UserTrackingId",
                        column: x => x.UserTrackingId,
                        principalTable: "user_trackings",
                        principalColumn: "UserTrackingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_daily_tracking_values",
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
                    table.PrimaryKey("PK_user_daily_tracking_values", x => new { x.UserId, x.Day, x.UserTrackingValueId, x.Occurrence });
                    table.ForeignKey(
                        name: "FK_user_daily_tracking_values_user_days_UserId_Day",
                        columns: x => new { x.UserId, x.Day },
                        principalTable: "user_days",
                        principalColumns: new[] { "UserId", "Day" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_daily_tracking_values_user_tracking_values_UserTrackin~",
                        column: x => x.UserTrackingValueId,
                        principalTable: "user_tracking_values",
                        principalColumn: "UserTrackingValueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_tracking_value_metadata",
                columns: table => new
                {
                    UserTrackingValueId = table.Column<int>(type: "integer", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_tracking_value_metadata", x => new { x.UserTrackingValueId, x.Key });
                    table.ForeignKey(
                        name: "FK_user_tracking_value_metadata_user_tracking_values_UserTrack~",
                        column: x => x.UserTrackingValueId,
                        principalTable: "user_tracking_values",
                        principalColumn: "UserTrackingValueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_daily_tracking_values_UserTrackingValueId",
                table: "user_daily_tracking_values",
                column: "UserTrackingValueId");

            migrationBuilder.CreateIndex(
                name: "IX_user_fuelings_UserId_Day",
                table: "user_fuelings",
                columns: new[] { "UserId", "Day" });

            migrationBuilder.CreateIndex(
                name: "IX_user_meals_UserId_Day",
                table: "user_meals",
                columns: new[] { "UserId", "Day" });

            migrationBuilder.CreateIndex(
                name: "IX_user_plans_PlanId",
                table: "user_plans",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_user_tracking_values_UserTrackingId",
                table: "user_tracking_values",
                column: "UserTrackingId");

            migrationBuilder.CreateIndex(
                name: "IX_user_trackings_UserId",
                table: "user_trackings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_victories_UserId",
                table: "victories",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fuelings");

            migrationBuilder.DropTable(
                name: "user_daily_tracking_values");

            migrationBuilder.DropTable(
                name: "user_fuelings");

            migrationBuilder.DropTable(
                name: "user_meals");

            migrationBuilder.DropTable(
                name: "user_plans");

            migrationBuilder.DropTable(
                name: "user_tracking_value_metadata");

            migrationBuilder.DropTable(
                name: "victories");

            migrationBuilder.DropTable(
                name: "user_days");

            migrationBuilder.DropTable(
                name: "plans");

            migrationBuilder.DropTable(
                name: "user_tracking_values");

            migrationBuilder.DropTable(
                name: "user_trackings");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
