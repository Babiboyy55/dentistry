using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nhakhoa.Migrations
{
    /// <inheritdoc />
    public partial class SeedVietnameseHolidays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "HolidayDates",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Date", "HolidayType", "Name", "Notes", "RepeatYearly" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 5, 28, 14, 58, 30, 555, DateTimeKind.Local).AddTicks(7228), "Hệ thống", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cố định", "Tết Dương Lịch", "Nghỉ Tết Dương Lịch hàng năm", true },
                    { 2, new DateTime(2026, 5, 28, 14, 58, 30, 555, DateTimeKind.Local).AddTicks(8134), "Hệ thống", new DateTime(2026, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cố định", "Ngày Giải phóng Miền Nam", "Kỷ niệm Ngày Giải phóng Miền Nam 30/4", true },
                    { 3, new DateTime(2026, 5, 28, 14, 58, 30, 555, DateTimeKind.Local).AddTicks(8136), "Hệ thống", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cố định", "Ngày Quốc tế Lao động", "Ngày Quốc tế Lao động 1/5", true },
                    { 4, new DateTime(2026, 5, 28, 14, 58, 30, 555, DateTimeKind.Local).AddTicks(8138), "Hệ thống", new DateTime(2026, 9, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cố định", "Ngày Quốc Khánh", "Ngày Quốc Khánh Việt Nam 2/9", true }
                });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 28, 14, 58, 30, 555, DateTimeKind.Local).AddTicks(6096));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 28, 14, 33, 50, 634, DateTimeKind.Local).AddTicks(6466));
        }
    }
}
