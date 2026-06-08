using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nhakhoa.Migrations
{
    /// <inheritdoc />
    public partial class AddDentalChairs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DoctorSpecialties",
                keyColumns: new[] { "SpecialtyId", "StaffProfileId" },
                keyValues: new object[] { 2, 203 });

            migrationBuilder.DeleteData(
                table: "DoctorSpecialties",
                keyColumns: new[] { "SpecialtyId", "StaffProfileId" },
                keyValues: new object[] { 1, 204 });

            migrationBuilder.AddColumn<int>(
                name: "DentalChairId",
                table: "Shifts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DentalChairId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DentalChairs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicId = table.Column<int>(type: "int", nullable: false),
                    ChairCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DentalChairs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DentalChairs_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Phòng khám Nha khoa tổng quát A1");

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Phòng khám Thẩm mỹ & Chỉnh nha B2");

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Phòng khám Cấy ghép Implant C1");

            migrationBuilder.InsertData(
                table: "DoctorSpecialties",
                columns: new[] { "SpecialtyId", "StaffProfileId" },
                values: new object[,]
                {
                    { 3, 203 },
                    { 4, 204 }
                });

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 7, 20, 46, 41, 432, DateTimeKind.Local).AddTicks(6619));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 7, 20, 46, 41, 432, DateTimeKind.Local).AddTicks(7563));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 7, 20, 46, 41, 432, DateTimeKind.Local).AddTicks(7565));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 7, 20, 46, 41, 432, DateTimeKind.Local).AddTicks(7581));

            migrationBuilder.UpdateData(
                table: "MedicalServices",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Department", "Description", "Name", "Price" },
                values: new object[] { "Khám bệnh", "Khám răng tổng quát, chụp phim X-quang răng và lên phác đồ điều trị.", "Khám & Tư vấn răng miệng", 100000m });

            migrationBuilder.UpdateData(
                table: "MedicalServices",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Department", "Description", "Name", "Price" },
                values: new object[] { "Chỉnh nha", "Điều chỉnh khớp cắn bằng hệ thống mắc cài kim loại cao cấp.", "Niềng răng mắc cài kim loại", 25000000m });

            migrationBuilder.UpdateData(
                table: "MedicalServices",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Department", "Description", "IsActive", "Name", "Price" },
                values: new object[] { "Nha khoa thẩm mỹ", "Phục hình răng sứt mẻ, ố vàng bằng răng toàn sứ Cercon nhập khẩu Đức.", true, "Răng sứ Cercon HT", 5000000m });

            migrationBuilder.UpdateData(
                table: "MedicalServices",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Department", "Description", "Name", "Price", "SpecialtyId" },
                values: new object[] { "Tiểu phẫu", "Nhổ răng khôn mọc ngầm, lệch bằng máy siêu âm Piezotome không đau, mau lành.", "Nhổ răng khôn Piezotome", 1500000m, 1 });

            migrationBuilder.UpdateData(
                table: "MedicalServices",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "DefaultWarrantyMonths", "Department", "Description", "Name", "Price" },
                values: new object[] { 120, "Cấy ghép răng", "Phục hình răng đã mất bằng chân răng nhân tạo Implant Dentium.", "Cấy ghép Implant Dentium", 18000000m });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "DentalChairId" },
                values: new object[] { new DateTime(2026, 6, 7, 20, 46, 41, 432, DateTimeKind.Local).AddTicks(5458), null });

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Code", "Description", "Name" },
                values: new object[] { "NKTQ", "Khám răng tổng quát, nhổ răng, chữa tủy và điều trị các bệnh lý răng miệng cơ bản.", "Nha khoa tổng quát" });

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Code", "Description", "Name" },
                values: new object[] { "RSTM", "Phục hình răng sứ thẩm mỹ, dán sứ Veneer siêu mỏng và tẩy trắng răng.", "Răng sứ thẩm mỹ" });

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Code", "Description", "Name" },
                values: new object[] { "CNNR", "Nắn chỉnh răng lệch lạc, răng thưa, hô, móm bằng khay trong suốt hoặc mắc cài.", "Chỉnh nha - Niềng răng" });

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Code", "Description", "Name" },
                values: new object[] { "IMPL", "Phục hình răng đã mất bằng chân răng nhân tạo Implant công nghệ hiện đại.", "Cấy ghép Implant" });

            migrationBuilder.UpdateData(
                table: "StaffProfiles",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "Department", "PositionTitle", "PrimaryClinic" },
                values: new object[] { "Khoa khám bệnh", "Nha sĩ Tổng quát", "Phòng khám Nha khoa tổng quát A1" });

            migrationBuilder.UpdateData(
                table: "StaffProfiles",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "Department", "PositionTitle", "PrimaryClinic" },
                values: new object[] { "Khoa thẩm mỹ", "Chuyên gia Phục hình răng", "Phòng khám Thẩm mỹ & Chỉnh nha B2" });

            migrationBuilder.UpdateData(
                table: "StaffProfiles",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "Department", "PositionTitle", "PrimaryClinic" },
                values: new object[] { "Khoa chỉnh nha", "Chuyên gia Chỉnh nha", "Phòng khám Thẩm mỹ & Chỉnh nha B2" });

            migrationBuilder.UpdateData(
                table: "StaffProfiles",
                keyColumn: "Id",
                keyValue: 204,
                columns: new[] { "Department", "PositionTitle", "PrimaryClinic" },
                values: new object[] { "Khoa cấy ghép", "Chuyên gia Cấy ghép Implant", "Phòng khám Cấy ghép Implant C1" });

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_DentalChairId",
                table: "Shifts",
                column: "DentalChairId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DentalChairId",
                table: "Appointments",
                column: "DentalChairId");

            migrationBuilder.CreateIndex(
                name: "IX_DentalChairs_ClinicId",
                table: "DentalChairs",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_DentalChairs_DentalChairId",
                table: "Appointments",
                column: "DentalChairId",
                principalTable: "DentalChairs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_DentalChairs_DentalChairId",
                table: "Shifts",
                column: "DentalChairId",
                principalTable: "DentalChairs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_DentalChairs_DentalChairId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_DentalChairs_DentalChairId",
                table: "Shifts");

            migrationBuilder.DropTable(
                name: "DentalChairs");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_DentalChairId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_DentalChairId",
                table: "Appointments");

            migrationBuilder.DeleteData(
                table: "DoctorSpecialties",
                keyColumns: new[] { "SpecialtyId", "StaffProfileId" },
                keyValues: new object[] { 3, 203 });

            migrationBuilder.DeleteData(
                table: "DoctorSpecialties",
                keyColumns: new[] { "SpecialtyId", "StaffProfileId" },
                keyValues: new object[] { 4, 204 });

            migrationBuilder.DropColumn(
                name: "DentalChairId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "DentalChairId",
                table: "Appointments");

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Phòng khám Tim mạch A1");

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Phòng khám Thần kinh B2");

            migrationBuilder.UpdateData(
                table: "Clinics",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Phòng khám Răng Hàm Mặt C1");

            migrationBuilder.InsertData(
                table: "DoctorSpecialties",
                columns: new[] { "SpecialtyId", "StaffProfileId" },
                values: new object[,]
                {
                    { 2, 203 },
                    { 1, 204 }
                });

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
                table: "MedicalServices",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Department", "Description", "Name", "Price" },
                values: new object[] { "Nội tổng quát", "Khám sàng lọc và tư vấn sức khỏe cơ bản", "Khám nội tổng quát", 500000m });

            migrationBuilder.UpdateData(
                table: "MedicalServices",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Department", "Description", "Name", "Price" },
                values: new object[] { "Xét nghiệm", "Phân tích huyết học tự động công nghệ cao", "Xét nghiệm công thức máu (24 chỉ số)", 150000m });

            migrationBuilder.UpdateData(
                table: "MedicalServices",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Department", "Description", "IsActive", "Name", "Price" },
                values: new object[] { "Chẩn đoán hình ảnh", "Siêu âm 4D ổ bụng và các cơ quan nội tạng", false, "Siêu âm bụng tổng quát", 350000m });

            migrationBuilder.UpdateData(
                table: "MedicalServices",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Department", "Description", "Name", "Price", "SpecialtyId" },
                values: new object[] { "Nha khoa tổng quát", "Nhổ răng khôn mọc lệch, mọc ngầm sử dụng sóng siêu âm Piezotome", "Nhổ răng khôn", 1200000m, 4 });

            migrationBuilder.UpdateData(
                table: "MedicalServices",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "DefaultWarrantyMonths", "Department", "Description", "Name", "Price" },
                values: new object[] { 12, "Nha khoa thẩm mỹ", "Tẩy trắng răng công nghệ Laser Whitening nhanh chóng, không ê buốt", "Tẩy trắng răng Laser", 2500000m });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 11, 13, 47, 93, DateTimeKind.Local).AddTicks(7778));

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Code", "Description", "Name" },
                values: new object[] { "CARD-001", "Chuyên chẩn đoán và điều trị các bệnh lý tim mạch và mạch máu.", "Tim mạch" });

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Code", "Description", "Name" },
                values: new object[] { "NEUR-002", "Điều trị các bệnh lý liên quan đến hệ thần kinh trung ương và ngoại biên.", "Thần kinh" });

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Code", "Description", "Name" },
                values: new object[] { "PEDI-003", "Chăm sóc sức khỏe toàn diện, sàng lọc phát triển thể chất ở trẻ em.", "Nhi khoa" });

            migrationBuilder.UpdateData(
                table: "Specialties",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Code", "Description", "Name" },
                values: new object[] { "DENT-004", "Điều trị và phục hình răng hàm mặt thẩm mỹ, công nghệ cao.", "Răng Hàm Mặt" });

            migrationBuilder.UpdateData(
                table: "StaffProfiles",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "Department", "PositionTitle", "PrimaryClinic" },
                values: new object[] { "Nội tổng quát", "Y học cổ truyền", "Phòng khám A1" });

            migrationBuilder.UpdateData(
                table: "StaffProfiles",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "Department", "PositionTitle", "PrimaryClinic" },
                values: new object[] { "Chẩn đoán hình ảnh", "Chẩn đoán hình ảnh", "Phòng B2" });

            migrationBuilder.UpdateData(
                table: "StaffProfiles",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "Department", "PositionTitle", "PrimaryClinic" },
                values: new object[] { "Thần kinh", "Thần kinh học", "Phòng C1" });

            migrationBuilder.UpdateData(
                table: "StaffProfiles",
                keyColumn: "Id",
                keyValue: 204,
                columns: new[] { "Department", "PositionTitle", "PrimaryClinic" },
                values: new object[] { "Tim mạch", "Chuyên gia Tim mạch", "Phòng khám A1" });
        }
    }
}
