using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nhakhoa.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialtyAndClinicSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SpecialtyId",
                table: "MedicalServices",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Specialties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clinics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DefaultSpecialtyId = table.Column<int>(type: "int", nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clinics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clinics_Specialties_DefaultSpecialtyId",
                        column: x => x.DefaultSpecialtyId,
                        principalTable: "Specialties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DoctorSpecialties",
                columns: table => new
                {
                    StaffProfileId = table.Column<int>(type: "int", nullable: false),
                    SpecialtyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorSpecialties", x => new { x.StaffProfileId, x.SpecialtyId });
                    table.ForeignKey(
                        name: "FK_DoctorSpecialties_Specialties_SpecialtyId",
                        column: x => x.SpecialtyId,
                        principalTable: "Specialties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorSpecialties_StaffProfiles_StaffProfileId",
                        column: x => x.StaffProfileId,
                        principalTable: "StaffProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicId = table.Column<int>(type: "int", nullable: false),
                    StaffProfileId = table.Column<int>(type: "int", nullable: false),
                    ShiftDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shifts_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Shifts_StaffProfiles_StaffProfileId",
                        column: x => x.StaffProfileId,
                        principalTable: "StaffProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "MedicalServices",
                keyColumn: "Id",
                keyValue: 1,
                column: "SpecialtyId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "MedicalServices",
                keyColumn: "Id",
                keyValue: 2,
                column: "SpecialtyId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "MedicalServices",
                keyColumn: "Id",
                keyValue: 3,
                column: "SpecialtyId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "MedicalServices",
                keyColumn: "Id",
                keyValue: 4,
                column: "SpecialtyId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "MedicalServices",
                keyColumn: "Id",
                keyValue: 5,
                column: "SpecialtyId",
                value: 4);

            migrationBuilder.InsertData(
                table: "Specialties",
                columns: new[] { "Id", "Code", "Description", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "CARD-001", "Chuyên chẩn đoán và điều trị các bệnh lý tim mạch và mạch máu.", "Tim mạch", new DateTime(2023, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "NEUR-002", "Điều trị các bệnh lý liên quan đến hệ thần kinh trung ương và ngoại biên.", "Thần kinh", new DateTime(2023, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, "PEDI-003", "Chăm sóc sức khỏe toàn diện, sàng lọc phát triển thể chất ở trẻ em.", "Nhi khoa", new DateTime(2023, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, "DENT-004", "Điều trị và phục hình răng hàm mặt thẩm mỹ, công nghệ cao.", "Răng Hàm Mặt", new DateTime(2023, 10, 18, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FullName", "IsActive", "IsTemporaryPassword", "PasswordHash", "PhoneNumber", "ResetOtpCode", "ResetOtpExpiry", "Role", "SecurityStamp", "Username" },
                values: new object[,]
                {
                    { 201, "dat.nguyen@clinic.com", "BS. Nguyễn Văn Đạt", true, false, "123456", "0987654321", null, null, "Doctor", "doctor-1-security-stamp", "doctor1" },
                    { 202, "mai.tran@clinic.com", "BS. Trần Thanh Mai", false, false, "123456", "0987654322", null, null, "Doctor", "doctor-2-security-stamp", "doctor2" },
                    { 203, "tuan.le@clinic.com", "BS. Lê Anh Tuấn", true, false, "123456", "0987654323", null, null, "Doctor", "doctor-3-security-stamp", "doctor3" },
                    { 204, "sarah.johnson@clinic.com", "BS. Sarah Johnson", true, false, "123456", "0987654324", null, null, "Doctor", "doctor-4-security-stamp", "doctor4" }
                });

            migrationBuilder.InsertData(
                table: "Clinics",
                columns: new[] { "Id", "Capacity", "DefaultSpecialtyId", "IsActive", "Location", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 15, 1, true, "Tầng 1 - Khu A", "Phòng khám Tim mạch A1", new DateTime(2023, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, 10, 2, true, "Tầng 2 - Khu B", "Phòng khám Thần kinh B2", new DateTime(2023, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, 20, 4, true, "Tầng 1 - Khu C", "Phòng khám Răng Hàm Mặt C1", new DateTime(2023, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "StaffProfiles",
                columns: new[] { "Id", "Address", "Department", "Gender", "JoinDate", "PositionTitle", "PrimaryClinic", "StaffCode", "UserId" },
                values: new object[,]
                {
                    { 201, "Hà Nội", "Nội tổng quát", "Nam", new DateTime(2022, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Y học cổ truyền", "Phòng khám A1", "DOC-102", 201 },
                    { 202, "Đà Nẵng", "Chẩn đoán hình ảnh", "Nữ", new DateTime(2021, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chẩn đoán hình ảnh", "Phòng B2", "DOC-205", 202 },
                    { 203, "Hồ Chí Minh", "Thần kinh", "Nam", new DateTime(2023, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thần kinh học", "Phòng C1", "DOC-098", 203 },
                    { 204, "Hà Nội", "Tim mạch", "Nữ", new DateTime(2023, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chuyên gia Tim mạch", "Phòng khám A1", "DOC-110", 204 }
                });

            migrationBuilder.InsertData(
                table: "DoctorSpecialties",
                columns: new[] { "SpecialtyId", "StaffProfileId" },
                values: new object[,]
                {
                    { 2, 203 },
                    { 1, 204 }
                });

            migrationBuilder.InsertData(
                table: "Shifts",
                columns: new[] { "Id", "ClinicId", "IsActive", "ShiftDate", "StaffProfileId" },
                values: new object[] { 1, 1, true, new DateTime(2026, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 204 });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalServices_SpecialtyId",
                table: "MedicalServices",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_DefaultSpecialtyId",
                table: "Clinics",
                column: "DefaultSpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorSpecialties_SpecialtyId",
                table: "DoctorSpecialties",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_ClinicId",
                table: "Shifts",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_StaffProfileId",
                table: "Shifts",
                column: "StaffProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalServices_Specialties_SpecialtyId",
                table: "MedicalServices",
                column: "SpecialtyId",
                principalTable: "Specialties",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalServices_Specialties_SpecialtyId",
                table: "MedicalServices");

            migrationBuilder.DropTable(
                name: "DoctorSpecialties");

            migrationBuilder.DropTable(
                name: "Shifts");

            migrationBuilder.DropTable(
                name: "Clinics");

            migrationBuilder.DropTable(
                name: "Specialties");

            migrationBuilder.DropIndex(
                name: "IX_MedicalServices_SpecialtyId",
                table: "MedicalServices");

            migrationBuilder.DeleteData(
                table: "StaffProfiles",
                keyColumn: "Id",
                keyValue: 201);

            migrationBuilder.DeleteData(
                table: "StaffProfiles",
                keyColumn: "Id",
                keyValue: 202);

            migrationBuilder.DeleteData(
                table: "StaffProfiles",
                keyColumn: "Id",
                keyValue: 203);

            migrationBuilder.DeleteData(
                table: "StaffProfiles",
                keyColumn: "Id",
                keyValue: 204);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 201);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 202);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 203);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 204);

            migrationBuilder.DropColumn(
                name: "SpecialtyId",
                table: "MedicalServices");
        }
    }
}
