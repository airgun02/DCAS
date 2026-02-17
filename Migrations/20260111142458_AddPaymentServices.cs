using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DCAS.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "TodaySchedule",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "TodaySchedule");
        }
    }
}
