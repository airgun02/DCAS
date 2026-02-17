using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DCAS.Migrations
{
    /// <inheritdoc />
    public partial class kskkss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payment_MedicineInventory_MedicineId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_PersonInfo_PersonInfoId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Services_ServicesId",
                table: "Payment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payment",
                table: "Payment");

            migrationBuilder.RenameTable(
                name: "Payment",
                newName: "Payments");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_ServicesId",
                table: "Payments",
                newName: "IX_Payments_ServicesId");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_PersonInfoId",
                table: "Payments",
                newName: "IX_Payments_PersonInfoId");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_MedicineId",
                table: "Payments",
                newName: "IX_Payments_MedicineId");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "MedicineInventory",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "MedicineInventory",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Miligram",
                table: "MedicineInventory",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "PaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_MedicineInventory_MedicineId",
                table: "Payments",
                column: "MedicineId",
                principalTable: "MedicineInventory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_PersonInfo_PersonInfoId",
                table: "Payments",
                column: "PersonInfoId",
                principalTable: "PersonInfo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Services_ServicesId",
                table: "Payments",
                column: "ServicesId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_MedicineInventory_MedicineId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_PersonInfo_PersonInfoId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Services_ServicesId",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "MedicineInventory");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "MedicineInventory");

            migrationBuilder.DropColumn(
                name: "Miligram",
                table: "MedicineInventory");

            migrationBuilder.RenameTable(
                name: "Payments",
                newName: "Payment");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_ServicesId",
                table: "Payment",
                newName: "IX_Payment_ServicesId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_PersonInfoId",
                table: "Payment",
                newName: "IX_Payment_PersonInfoId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_MedicineId",
                table: "Payment",
                newName: "IX_Payment_MedicineId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payment",
                table: "Payment",
                column: "PaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_MedicineInventory_MedicineId",
                table: "Payment",
                column: "MedicineId",
                principalTable: "MedicineInventory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_PersonInfo_PersonInfoId",
                table: "Payment",
                column: "PersonInfoId",
                principalTable: "PersonInfo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Services_ServicesId",
                table: "Payment",
                column: "ServicesId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
