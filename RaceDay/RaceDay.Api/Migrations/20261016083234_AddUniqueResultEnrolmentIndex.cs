using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.Api.Migrations
{
    public partial class AddUniqueResultEnrolmentIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RESULT_EnrolmentID",
                schema: "dbo",
                table: "RESULT",
                column: "EnrolmentID",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RESULT_EnrolmentID",
                schema: "dbo",
                table: "RESULT");
        }
    }
}