using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nhakhoa.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicalServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalServices", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "MedicalServices",
                columns: new[] { "Id", "Department", "Description", "IsActive", "Name", "Price", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Nội tổng quát", "Khám sàng lọc và tư vấn sức khỏe cơ bản", true, "Khám nội tổng quát", 500000m, new DateTime(2023, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "Xét nghiệm", "Phân tích huyết học tự động công nghệ cao", true, "Xét nghiệm công thức máu (24 chỉ số)", 150000m, new DateTime(2023, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, "Chẩn đoán hình ảnh", "Siêu âm 4D ổ bụng và các cơ quan nội tạng", false, "Siêu âm bụng tổng quát", 350000m, new DateTime(2023, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, "Nha khoa tổng quát", "Nhổ răng khôn mọc lệch, mọc ngầm sử dụng sóng siêu âm Piezotome", true, "Nhổ răng khôn", 1200000m, new DateTime(2023, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, "Nha khoa thẩm mỹ", "Tẩy trắng răng công nghệ Laser Whitening nhanh chóng, không ê buốt", true, "Tẩy trắng răng Laser", 2500000m, new DateTime(2023, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicalServices");
        }
    }
}
