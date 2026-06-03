using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nhakhoa.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentConcurrencyStampAndUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_StaffProfileId",
                table: "Appointments");

            migrationBuilder.AddColumn<Guid>(
                name: "ConcurrencyStamp",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 3, 20, 32, 16, 242, DateTimeKind.Local).AddTicks(1535));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 3, 20, 32, 16, 242, DateTimeKind.Local).AddTicks(7785));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 3, 20, 32, 16, 242, DateTimeKind.Local).AddTicks(7805));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 3, 20, 32, 16, 242, DateTimeKind.Local).AddTicks(7812));

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 3, 20, 32, 16, 241, DateTimeKind.Local).AddTicks(6153));

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_StaffProfileId_AppointmentDate_TimeSlot",
                table: "Appointments",
                columns: new[] { "StaffProfileId", "AppointmentDate", "TimeSlot" },
                unique: true,
                filter: "[Status] != 'Đã hủy'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_StaffProfileId_AppointmentDate_TimeSlot",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "Appointments");

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 28, 15, 5, 8, 359, DateTimeKind.Local).AddTicks(8550));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 28, 15, 5, 8, 359, DateTimeKind.Local).AddTicks(9470));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 28, 15, 5, 8, 359, DateTimeKind.Local).AddTicks(9473));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 28, 15, 5, 8, 359, DateTimeKind.Local).AddTicks(9474));

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 28, 15, 5, 8, 359, DateTimeKind.Local).AddTicks(7461));

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_StaffProfileId",
                table: "Appointments",
                column: "StaffProfileId");
        }
    }
}
