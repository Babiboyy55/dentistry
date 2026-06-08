using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nhakhoa.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethodCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsDigitalGateway = table.Column<bool>(type: "bit", nullable: false),
                    MerchantId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SecretKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Environment = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EndpointUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 7, 21, 5, 30, 160, DateTimeKind.Local).AddTicks(7999));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 7, 21, 5, 30, 160, DateTimeKind.Local).AddTicks(8914));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 7, 21, 5, 30, 160, DateTimeKind.Local).AddTicks(8917));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 7, 21, 5, 30, 160, DateTimeKind.Local).AddTicks(8918));

            migrationBuilder.InsertData(
                table: "Invoices",
                columns: new[] { "Id", "CreatedBy", "InvoiceCode", "IssuedAt", "Notes", "PatientId", "PaymentMethodCode", "Status", "TotalAmount" },
                values: new object[] { 1, "admin", "HD-CASH-TEST", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hóa đơn thử nghiệm tiền mặt", null, "CASH", "Chờ thanh toán", 500000m });

            migrationBuilder.InsertData(
                table: "PaymentMethods",
                columns: new[] { "Id", "Code", "EndpointUrl", "Environment", "IsDigitalGateway", "IsEnabled", "MerchantId", "Name", "SecretKey", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "CASH", null, null, false, true, null, "Tiền mặt", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "BANK", null, null, false, true, null, "Chuyển khoản", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, "VNPAY", "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html", "Sandbox", true, false, null, "VNPay", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, "MOMO", "https://test-payment.momo.vn/v2/gateway/api/create", "Sandbox", true, false, null, "MoMo", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, "INSURANCE", null, null, false, true, null, "Bảo hiểm y tế", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 7, 21, 5, 30, 160, DateTimeKind.Local).AddTicks(6900));

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PatientId",
                table: "Invoices",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "PaymentMethods");

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
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 7, 20, 46, 41, 432, DateTimeKind.Local).AddTicks(5458));
        }
    }
}
