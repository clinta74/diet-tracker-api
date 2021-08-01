using Microsoft.EntityFrameworkCore.Migrations;

namespace diet_tracker_api.Migrations
{
    public partial class metadatatodictionary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "UserTrackingValue");

            migrationBuilder.CreateTable(
                name: "UserTrackingValueMetadata",
                columns: table => new
                {
                    UserTrackingValueId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTrackingValueMetadata", x => new { x.UserTrackingValueId, x.Key });
                    table.ForeignKey(
                        name: "FK_UserTrackingValueMetadata_UserTrackingValue_UserTrackingValueId",
                        column: x => x.UserTrackingValueId,
                        principalTable: "UserTrackingValue",
                        principalColumn: "UserTrackingValueId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTrackingValueMetadata");

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "UserTrackingValue",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
