using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DCAS.Migrations
{
    /// <inheritdoc />
    public partial class DB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMedicine_MedicineInventory_MedicineId",
                table: "PaymentMedicine");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMedicine_MedicineId",
                table: "PaymentMedicine");

            migrationBuilder.DropColumn(
                name: "MedicineId",
                table: "PaymentMedicine");

            migrationBuilder.AddColumn<string>(
                name: "MedicineName",
                table: "PaymentMedicine",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MedicineName",
                table: "PaymentMedicine");

            migrationBuilder.AddColumn<int>(
                name: "MedicineId",
                table: "PaymentMedicine",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMedicine_MedicineId",
                table: "PaymentMedicine",
                column: "MedicineId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMedicine_MedicineInventory_MedicineId",
                table: "PaymentMedicine",
                column: "MedicineId",
                principalTable: "MedicineInventory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
