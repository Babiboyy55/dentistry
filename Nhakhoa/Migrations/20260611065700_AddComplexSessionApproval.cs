using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nhakhoa.Migrations
{
    /// <inheritdoc />
    public partial class AddComplexSessionApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminNote",
                table: "ExaminationSessions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplexReason",
                table: "ExaminationSessions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplexStatus",
                table: "ExaminationSessions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RequestedCoefficient",
                table: "ExaminationSessions",
                type: "decimal(3,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "ExaminationSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 11, 13, 56, 59, 938, DateTimeKind.Local).AddTicks(4169));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 11, 13, 56, 59, 938, DateTimeKind.Local).AddTicks(5034));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 11, 13, 56, 59, 938, DateTimeKind.Local).AddTicks(5036));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 11, 13, 56, 59, 938, DateTimeKind.Local).AddTicks(5037));

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 11, 13, 56, 59, 938, DateTimeKind.Local).AddTicks(3057));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminNote",
                table: "ExaminationSessions");

            migrationBuilder.DropColumn(
                name: "ComplexReason",
                table: "ExaminationSessions");

            migrationBuilder.DropColumn(
                name: "ComplexStatus",
                table: "ExaminationSessions");

            migrationBuilder.DropColumn(
                name: "RequestedCoefficient",
                table: "ExaminationSessions");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "ExaminationSessions");

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 11, 13, 9, 25, 365, DateTimeKind.Local).AddTicks(5362));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 11, 13, 9, 25, 365, DateTimeKind.Local).AddTicks(6276));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 11, 13, 9, 25, 365, DateTimeKind.Local).AddTicks(6279));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 11, 13, 9, 25, 365, DateTimeKind.Local).AddTicks(6283));

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 11, 13, 9, 25, 365, DateTimeKind.Local).AddTicks(4219));
        }
    }
}
