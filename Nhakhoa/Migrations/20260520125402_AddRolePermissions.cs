using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nhakhoa.Migrations
{
    /// <inheritdoc />
    public partial class AddRolePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModuleKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModuleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ModuleIcon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsAllowed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "IsAllowed", "ModuleIcon", "ModuleKey", "ModuleName", "Role", "SortOrder" },
                values: new object[,]
                {
                    { 1, true, "bi-person-lock", "account_rbac", "Quản lý tài khoản & RBAC", "Admin", 1 },
                    { 2, true, "bi-journal-check", "audit_log", "Audit log & báo cáo hệ thống", "Admin", 2 },
                    { 3, true, "bi-gear", "system_config", "Cấu hình hệ thống", "Admin", 3 },
                    { 4, true, "bi-file-medical", "emr", "Bệnh án điện tử (EMR)", "Admin", 4 },
                    { 5, true, "bi-capsule", "prescription", "Kê đơn thuốc", "Admin", 5 },
                    { 6, true, "bi-eyedropper", "lab_test", "Chỉ định & kết quả xét nghiệm", "Admin", 6 },
                    { 7, true, "bi-calendar-event", "schedule_view", "Lịch khám cá nhân (read)", "Admin", 7 },
                    { 8, true, "bi-calendar-check", "appointment", "Quản lý lịch hẹn", "Admin", 8 },
                    { 9, true, "bi-person-plus", "patient_reg", "Đăng ký / tìm kiếm bệnh nhân", "Admin", 9 },
                    { 10, true, "bi-receipt", "invoice", "Tạo hóa đơn & thu tiền", "Admin", 10 },
                    { 11, true, "bi-clipboard2-data", "patient_admin", "Thông tin hành chính bệnh nhân", "Admin", 11 },
                    { 12, false, "bi-person-lock", "account_rbac", "Quản lý tài khoản & RBAC", "Doctor", 1 },
                    { 13, false, "bi-journal-check", "audit_log", "Audit log & báo cáo hệ thống", "Doctor", 2 },
                    { 14, false, "bi-gear", "system_config", "Cấu hình hệ thống", "Doctor", 3 },
                    { 15, true, "bi-file-medical", "emr", "Bệnh án điện tử (EMR)", "Doctor", 4 },
                    { 16, true, "bi-capsule", "prescription", "Kê đơn thuốc", "Doctor", 5 },
                    { 17, true, "bi-eyedropper", "lab_test", "Chỉ định & kết quả xét nghiệm", "Doctor", 6 },
                    { 18, true, "bi-calendar-event", "schedule_view", "Lịch khám cá nhân (read)", "Doctor", 7 },
                    { 19, false, "bi-calendar-check", "appointment", "Quản lý lịch hẹn", "Doctor", 8 },
                    { 20, false, "bi-person-plus", "patient_reg", "Đăng ký / tìm kiếm bệnh nhân", "Doctor", 9 },
                    { 21, false, "bi-receipt", "invoice", "Tạo hóa đơn & thu tiền", "Doctor", 10 },
                    { 22, false, "bi-clipboard2-data", "patient_admin", "Thông tin hành chính bệnh nhân", "Doctor", 11 },
                    { 23, false, "bi-person-lock", "account_rbac", "Quản lý tài khoản & RBAC", "Receptionist", 1 },
                    { 24, false, "bi-journal-check", "audit_log", "Audit log & báo cáo hệ thống", "Receptionist", 2 },
                    { 25, false, "bi-gear", "system_config", "Cấu hình hệ thống", "Receptionist", 3 },
                    { 26, false, "bi-file-medical", "emr", "Bệnh án điện tử (EMR)", "Receptionist", 4 },
                    { 27, false, "bi-capsule", "prescription", "Kê đơn thuốc", "Receptionist", 5 },
                    { 28, false, "bi-eyedropper", "lab_test", "Chỉ định & kết quả xét nghiệm", "Receptionist", 6 },
                    { 29, false, "bi-calendar-event", "schedule_view", "Lịch khám cá nhân (read)", "Receptionist", 7 },
                    { 30, true, "bi-calendar-check", "appointment", "Quản lý lịch hẹn", "Receptionist", 8 },
                    { 31, true, "bi-person-plus", "patient_reg", "Đăng ký / tìm kiếm bệnh nhân", "Receptionist", 9 },
                    { 32, true, "bi-receipt", "invoice", "Tạo hóa đơn & thu tiền", "Receptionist", 10 },
                    { 33, true, "bi-clipboard2-data", "patient_admin", "Thông tin hành chính bệnh nhân", "Receptionist", 11 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolePermissions");
        }
    }
}
