using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nhakhoa.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffProfileExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRankChangePending",
                table: "StaffSalaryInfos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PendingRankTitle",
                table: "StaffSalaryInfos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcademicDegree",
                table: "StaffProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcademicRank",
                table: "StaffProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cccd",
                table: "StaffProfiles",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CchnExpiryDate",
                table: "StaffProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CchnIssueDate",
                table: "StaffProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CchnNumber",
                table: "StaffProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CchnProvider",
                table: "StaffProfiles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "StaffProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExperienceYears",
                table: "StaffProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobRank",
                table: "StaffProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "StaffProfiles",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "AcademicDegree", "AcademicRank", "Cccd", "CchnExpiryDate", "CchnIssueDate", "CchnNumber", "CchnProvider", "DateOfBirth", "ExperienceYears", "JobRank" },
                values: new object[] { "Bác sĩ thường", "Không", "123456789012", new DateTime(2035, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2015, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "CCHN-002341", "Sở Y tế Hà Nội", new DateTime(1980, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 8, "Bác sĩ" });

            migrationBuilder.UpdateData(
                table: "StaffProfiles",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "AcademicDegree", "AcademicRank", "Cccd", "CchnExpiryDate", "CchnIssueDate", "CchnNumber", "CchnProvider", "DateOfBirth", "ExperienceYears", "JobRank" },
                values: new object[] { "Bác sĩ chuyên khoa I", "Không", "234567890123", new DateTime(2038, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2018, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "CCHN-009842", "Sở Y tế Đà Nẵng", new DateTime(1985, 10, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, "Bác sĩ" });

            migrationBuilder.UpdateData(
                table: "StaffProfiles",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "AcademicDegree", "AcademicRank", "Cccd", "CchnExpiryDate", "CchnIssueDate", "CchnNumber", "CchnProvider", "DateOfBirth", "ExperienceYears", "JobRank" },
                values: new object[] { "Bác sĩ thường", "Không", "345678901234", new DateTime(2032, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2012, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "CCHN-005612", "Bộ Y tế", new DateTime(1978, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 12, "Bác sĩ chính" });

            migrationBuilder.UpdateData(
                table: "StaffProfiles",
                keyColumn: "Id",
                keyValue: 204,
                columns: new[] { "AcademicDegree", "AcademicRank", "Cccd", "CchnExpiryDate", "CchnIssueDate", "CchnNumber", "CchnProvider", "DateOfBirth", "ExperienceYears", "JobRank" },
                values: new object[] { "Bác sĩ chuyên khoa II", "Không", "001085002931", new DateTime(2026, 6, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2016, 6, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "CCHN-007788", "Sở Y tế Hà Nội", new DateTime(1988, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 15, "Bác sĩ cao cấp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRankChangePending",
                table: "StaffSalaryInfos");

            migrationBuilder.DropColumn(
                name: "PendingRankTitle",
                table: "StaffSalaryInfos");

            migrationBuilder.DropColumn(
                name: "AcademicDegree",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "AcademicRank",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "Cccd",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "CchnExpiryDate",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "CchnIssueDate",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "CchnNumber",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "CchnProvider",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "ExperienceYears",
                table: "StaffProfiles");

            migrationBuilder.DropColumn(
                name: "JobRank",
                table: "StaffProfiles");
        }
    }
}
