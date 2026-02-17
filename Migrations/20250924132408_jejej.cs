using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DCAS.Migrations
{
    /// <inheritdoc />
    public partial class jejej : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicineInventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicineName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicineInventory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetailOnee = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomeAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BirthDay = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Age = table.Column<int>(type: "int", nullable: true),
                    HomeNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Occupation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfficeAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameOfSpouse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersonalResponsibleforAccount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Relationship = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetailTwoo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhysicianCare = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhysicianName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicalServices = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DetailOne = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetailTwo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetailThree = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetailFour = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetailFive = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetailSix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetailSeven = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetailEigth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AvailableDay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AvailableTime = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicalNameService = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TodaySchedule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    PersonName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodaySchedule", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UsersId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UsersId);
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cash = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DoctorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountChanged = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DentistFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PersonInfoId = table.Column<int>(type: "int", nullable: false),
                    ServicesId = table.Column<int>(type: "int", nullable: false),
                    MedicineId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payment_MedicineInventory_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "MedicineInventory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payment_PersonInfo_PersonInfoId",
                        column: x => x.PersonInfoId,
                        principalTable: "PersonInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payment_Services_ServicesId",
                        column: x => x.ServicesId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payment_MedicineId",
                table: "Payment",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_PersonInfoId",
                table: "Payment",
                column: "PersonInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_ServicesId",
                table: "Payment",
                column: "ServicesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "TodaySchedule");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "MedicineInventory");

            migrationBuilder.DropTable(
                name: "PersonInfo");

            migrationBuilder.DropTable(
                name: "Services");
        }
    }
}
