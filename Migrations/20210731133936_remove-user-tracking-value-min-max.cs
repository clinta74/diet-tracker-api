using Microsoft.EntityFrameworkCore.Migrations;

namespace diet_tracker_api.Migrations
{
    public partial class Removeusertrackingvalueminmax : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Max",
                table: "UserTrackingValue");

            migrationBuilder.DropColumn(
                name: "Min",
                table: "UserTrackingValue");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Max",
                table: "UserTrackingValue",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Min",
                table: "UserTrackingValue",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
