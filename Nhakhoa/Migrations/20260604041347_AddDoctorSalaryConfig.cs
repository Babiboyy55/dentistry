using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nhakhoa.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorSalaryConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DoctorSalaryConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DegreeUniversity = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DegreeMaster = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DegreeDoctorate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DegreeAssocProf = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DegreeProfessor = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MultiplierMonday = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MultiplierTuesday = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MultiplierWednesday = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MultiplierThursday = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MultiplierFriday = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MultiplierSaturday = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MultiplierSunday = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorSalaryConfigs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DoctorSalaryConfigs",
                columns: new[] { "Id", "DegreeAssocProf", "DegreeDoctorate", "DegreeMaster", "DegreeProfessor", "DegreeUniversity", "HourlyRate", "MultiplierFriday", "MultiplierMonday", "MultiplierSaturday", "MultiplierSunday", "MultiplierThursday", "MultiplierTuesday", "MultiplierWednesday", "UpdatedAt" },
                values: new object[] { 1, 2.50m, 2.00m, 1.50m, 3.00m, 1.20m, 210000m, 1.00m, 1.00m, 1.20m, 1.50m, 1.00m, 1.00m, 1.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 11, 13, 47, 93, DateTimeKind.Local).AddTicks(8799));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 11, 13, 47, 93, DateTimeKind.Local).AddTicks(9739));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 11, 13, 47, 93, DateTimeKind.Local).AddTicks(9742));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 11, 13, 47, 93, DateTimeKind.Local).AddTicks(9744));

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 11, 13, 47, 93, DateTimeKind.Local).AddTicks(7778));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorSalaryConfigs");

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 9, 47, 11, 87, DateTimeKind.Local).AddTicks(7910));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 9, 47, 11, 87, DateTimeKind.Local).AddTicks(9054));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 9, 47, 11, 87, DateTimeKind.Local).AddTicks(9057));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 9, 47, 11, 87, DateTimeKind.Local).AddTicks(9058));

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 9, 47, 11, 87, DateTimeKind.Local).AddTicks(6898));
        }
    }
}
