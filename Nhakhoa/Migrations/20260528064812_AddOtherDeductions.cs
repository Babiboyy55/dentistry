using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nhakhoa.Migrations
{
    /// <inheritdoc />
    public partial class AddOtherDeductions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OtherDeductions",
                table: "StaffSalaryInfos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtherDeductions",
                table: "StaffSalaryInfos");
        }
    }
}
