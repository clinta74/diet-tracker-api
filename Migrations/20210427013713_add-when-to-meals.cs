using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace diet_tracker_api.Migrations
{
    public partial class addwhentomeals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Calories",
                table: "UserMeal");

            migrationBuilder.AddColumn<DateTime>(
                name: "When",
                table: "UserMeal",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "When",
                table: "UserMeal");

            migrationBuilder.AddColumn<decimal>(
                name: "Calories",
                table: "UserMeal",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
