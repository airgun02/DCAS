using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DCAS.Migrations
{
    /// <inheritdoc />
    public partial class sdasdsa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_MedicineInventory_MedicineId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_MedicineId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "MedicineId",
                table: "Payments");

            migrationBuilder.AddColumn<string>(
                name: "Age",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "Profile",
                table: "Users",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Specialization",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "WorkStatus",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PaymentMedicine",
                columns: table => new
                {
                    PaymentMedicineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicineId = table.Column<int>(type: "int", nullable: false),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMedicine", x => x.PaymentMedicineId);
                    table.ForeignKey(
                        name: "FK_PaymentMedicine_MedicineInventory_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "MedicineInventory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentMedicine_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMedicine_MedicineId",
                table: "PaymentMedicine",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMedicine_PaymentId",
                table: "PaymentMedicine",
                column: "PaymentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentMedicine");

            migrationBuilder.DropColumn(
                name: "Age",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Profile",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Specialization",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WorkStatus",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "MedicineId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MedicineId",
                table: "Payments",
                column: "MedicineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_MedicineInventory_MedicineId",
                table: "Payments",
                column: "MedicineId",
                principalTable: "MedicineInventory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
