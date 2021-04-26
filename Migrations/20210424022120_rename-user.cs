using Microsoft.EntityFrameworkCore.Migrations;

namespace diet_tracker_api.Migrations
{
    public partial class renameuser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlanId",
                table: "User");

            migrationBuilder.RenameColumn(
                name: "FristName",
                table: "User",
                newName: "FirstName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "User",
                newName: "FristName");

            migrationBuilder.AddColumn<int>(
                name: "PlanId",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
