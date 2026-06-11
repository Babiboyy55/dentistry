using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nhakhoa.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientPortalPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 11, 0, 1, 30, 6, DateTimeKind.Local).AddTicks(9927));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 11, 0, 1, 30, 7, DateTimeKind.Local).AddTicks(824));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 11, 0, 1, 30, 7, DateTimeKind.Local).AddTicks(828));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 11, 0, 1, 30, 7, DateTimeKind.Local).AddTicks(830));

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "IsAllowed", "ModuleIcon", "ModuleKey", "ModuleName", "Role", "SortOrder" },
                values: new object[] { true, "bi-box-seam", "medicine_inventory", "Quản lý kho thuốc", "Admin", 12 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-person-lock", "account_rbac", "Quản lý tài khoản & RBAC", 1 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-journal-check", "audit_log", "Audit log & báo cáo hệ thống", 2 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "IsAllowed", "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { false, "bi-gear", "system_config", "Cấu hình hệ thống", 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-file-medical", "emr", "Bệnh án điện tử (EMR)", 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-capsule", "prescription", "Kê đơn thuốc", 5 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-eyedropper", "lab_test", "Chỉ định & kết quả xét nghiệm", 6 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "IsAllowed", "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { true, "bi-calendar-event", "schedule_view", "Lịch khám cá nhân (read)", 7 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-calendar-check", "appointment", "Quản lý lịch hẹn", 8 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-person-plus", "patient_reg", "Đăng ký / tìm kiếm bệnh nhân", 9 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-receipt", "invoice", "Tạo hóa đơn & thu tiền", 10 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "Role", "SortOrder" },
                values: new object[] { "bi-clipboard2-data", "patient_admin", "Thông tin hành chính bệnh nhân", "Doctor", 11 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "Role", "SortOrder" },
                values: new object[] { "bi-box-seam", "medicine_inventory", "Quản lý kho thuốc", "Doctor", 12 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-person-lock", "account_rbac", "Quản lý tài khoản & RBAC", 1 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-journal-check", "audit_log", "Audit log & báo cáo hệ thống", 2 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-gear", "system_config", "Cấu hình hệ thống", 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-file-medical", "emr", "Bệnh án điện tử (EMR)", 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-capsule", "prescription", "Kê đơn thuốc", 5 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "IsAllowed", "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { false, "bi-eyedropper", "lab_test", "Chỉ định & kết quả xét nghiệm", 6 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "IsAllowed", "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { false, "bi-calendar-event", "schedule_view", "Lịch khám cá nhân (read)", 7 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-calendar-check", "appointment", "Quản lý lịch hẹn", 8 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-person-plus", "patient_reg", "Đăng ký / tìm kiếm bệnh nhân", 9 });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "IsAllowed", "ModuleIcon", "ModuleKey", "ModuleName", "Role", "SortOrder" },
                values: new object[,]
                {
                    { 34, true, "bi-receipt", "invoice", "Tạo hóa đơn & thu tiền", "Receptionist", 10 },
                    { 35, true, "bi-clipboard2-data", "patient_admin", "Thông tin hành chính bệnh nhân", "Receptionist", 11 },
                    { 36, false, "bi-box-seam", "medicine_inventory", "Quản lý kho thuốc", "Receptionist", 12 }
                });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 11, 0, 1, 30, 6, DateTimeKind.Local).AddTicks(8809));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 10, 23, 3, 14, 954, DateTimeKind.Local).AddTicks(6150));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 10, 23, 3, 14, 954, DateTimeKind.Local).AddTicks(7046));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 10, 23, 3, 14, 954, DateTimeKind.Local).AddTicks(7049));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 10, 23, 3, 14, 954, DateTimeKind.Local).AddTicks(7050));

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "IsAllowed", "ModuleIcon", "ModuleKey", "ModuleName", "Role", "SortOrder" },
                values: new object[] { false, "bi-person-lock", "account_rbac", "Quản lý tài khoản & RBAC", "Doctor", 1 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-journal-check", "audit_log", "Audit log & báo cáo hệ thống", 2 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-gear", "system_config", "Cấu hình hệ thống", 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "IsAllowed", "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { true, "bi-file-medical", "emr", "Bệnh án điện tử (EMR)", 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-capsule", "prescription", "Kê đơn thuốc", 5 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-eyedropper", "lab_test", "Chỉ định & kết quả xét nghiệm", 6 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-calendar-event", "schedule_view", "Lịch khám cá nhân (read)", 7 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "IsAllowed", "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { false, "bi-calendar-check", "appointment", "Quản lý lịch hẹn", 8 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-person-plus", "patient_reg", "Đăng ký / tìm kiếm bệnh nhân", 9 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-receipt", "invoice", "Tạo hóa đơn & thu tiền", 10 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-clipboard2-data", "patient_admin", "Thông tin hành chính bệnh nhân", 11 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "Role", "SortOrder" },
                values: new object[] { "bi-person-lock", "account_rbac", "Quản lý tài khoản & RBAC", "Receptionist", 1 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "Role", "SortOrder" },
                values: new object[] { "bi-journal-check", "audit_log", "Audit log & báo cáo hệ thống", "Receptionist", 2 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-gear", "system_config", "Cấu hình hệ thống", 3 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-file-medical", "emr", "Bệnh án điện tử (EMR)", 4 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-capsule", "prescription", "Kê đơn thuốc", 5 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-eyedropper", "lab_test", "Chỉ định & kết quả xét nghiệm", 6 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-calendar-event", "schedule_view", "Lịch khám cá nhân (read)", 7 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "IsAllowed", "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { true, "bi-calendar-check", "appointment", "Quản lý lịch hẹn", 8 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "IsAllowed", "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { true, "bi-person-plus", "patient_reg", "Đăng ký / tìm kiếm bệnh nhân", 9 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-receipt", "invoice", "Tạo hóa đơn & thu tiền", 10 });

            migrationBuilder.UpdateData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "ModuleIcon", "ModuleKey", "ModuleName", "SortOrder" },
                values: new object[] { "bi-clipboard2-data", "patient_admin", "Thông tin hành chính bệnh nhân", 11 });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 10, 23, 3, 14, 954, DateTimeKind.Local).AddTicks(4976));
        }
    }
}
