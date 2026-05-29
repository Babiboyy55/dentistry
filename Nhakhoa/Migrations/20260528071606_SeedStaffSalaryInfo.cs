using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nhakhoa.Migrations
{
    /// <inheritdoc />
    public partial class SeedStaffSalaryInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "StaffSalaryInfos",
                columns: new[] { "Id", "BaseSalary", "DegreeMultiplier", "DegreeTitle", "IsRankChangePending", "MonthlyBonus", "OtherDeductions", "PendingRankTitle", "RankMultiplier", "RankTitle", "SeniorityAllowance", "SpecializationAllowance", "UserId" },
                values: new object[,]
                {
                    { 1, 12000000m, 1.00m, "Bác sĩ thường", false, 1200000m, 100000m, null, 1.00m, "Bác sĩ", 1500000m, 2500000m, 201 },
                    { 2, 15000000m, 1.30m, "Bác sĩ chuyên khoa I", false, 2500000m, 0m, null, 1.00m, "Bác sĩ", 2000000m, 3500000m, 202 },
                    { 3, 18000000m, 1.00m, "Bác sĩ thường", false, 3000000m, 200000m, null, 1.20m, "Bác sĩ chính", 3500000m, 4000000m, 203 },
                    { 4, 25000000m, 1.40m, "Bác sĩ chuyên khoa II", false, 4500000m, 0m, null, 1.30m, "Bác sĩ cao cấp", 5500000m, 5000000m, 204 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StaffSalaryInfos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "StaffSalaryInfos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "StaffSalaryInfos",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "StaffSalaryInfos",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
