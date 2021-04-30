using Microsoft.EntityFrameworkCore.Migrations;

namespace diet_tracker_api.Migrations
{
    public partial class addstartuserplan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPlan",
                table: "UserPlan");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPlan",
                table: "UserPlan",
                columns: new[] { "UserId", "PlanId", "Start" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPlan",
                table: "UserPlan");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPlan",
                table: "UserPlan",
                columns: new[] { "UserId", "PlanId" });
        }
    }
}
