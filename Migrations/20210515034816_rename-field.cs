using Microsoft.EntityFrameworkCore.Migrations;

namespace diet_tracker_api.Migrations
{
    public partial class renamefield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Occurances",
                table: "UserTracking",
                newName: "Occurrences");

            migrationBuilder.RenameColumn(
                name: "Occurance",
                table: "UserDailyTracking",
                newName: "Occurrence");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Occurrences",
                table: "UserTracking",
                newName: "Occurances");

            migrationBuilder.RenameColumn(
                name: "Occurrence",
                table: "UserDailyTracking",
                newName: "Occurance");
        }
    }
}
