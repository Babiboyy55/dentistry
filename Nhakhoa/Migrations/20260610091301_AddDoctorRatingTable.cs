using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nhakhoa.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorRatingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DoctorRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExaminationSessionId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    Stars = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorRatings_ExaminationSessions_ExaminationSessionId",
                        column: x => x.ExaminationSessionId,
                        principalTable: "ExaminationSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorRatings_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DoctorRatings_StaffProfiles_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "StaffProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 10, 16, 13, 0, 319, DateTimeKind.Local).AddTicks(1713));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 10, 16, 13, 0, 319, DateTimeKind.Local).AddTicks(3574));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 10, 16, 13, 0, 319, DateTimeKind.Local).AddTicks(3586));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 10, 16, 13, 0, 319, DateTimeKind.Local).AddTicks(3588));

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 10, 16, 13, 0, 318, DateTimeKind.Local).AddTicks(6608));

            migrationBuilder.CreateIndex(
                name: "IX_DoctorRatings_DoctorId",
                table: "DoctorRatings",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorRatings_ExaminationSessionId",
                table: "DoctorRatings",
                column: "ExaminationSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorRatings_PatientId",
                table: "DoctorRatings",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorRatings");

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 7, 22, 40, 48, 849, DateTimeKind.Local).AddTicks(8830));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 7, 22, 40, 48, 849, DateTimeKind.Local).AddTicks(9878));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 7, 22, 40, 48, 849, DateTimeKind.Local).AddTicks(9881));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 7, 22, 40, 48, 849, DateTimeKind.Local).AddTicks(9883));

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 7, 22, 40, 48, 849, DateTimeKind.Local).AddTicks(7488));
        }
    }
}
