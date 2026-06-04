using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nhakhoa.Migrations
{
    /// <inheritdoc />
    public partial class AddUC4MissingModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultWarrantyMonths",
                table: "MedicalServices",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DentalWarranties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    MedicalServiceId = table.Column<int>(type: "int", nullable: false),
                    WarrantyCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Terms = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OverrideReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DentalWarranties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DentalWarranties_MedicalServices_MedicalServiceId",
                        column: x => x.MedicalServiceId,
                        principalTable: "MedicalServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DentalWarranties_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DentalWarranties_StaffProfiles_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "StaffProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExaminationSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    AppointmentId = table.Column<int>(type: "int", nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ClinicalNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TreatmentPlanSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    HomeCareInstructions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PatientCoefficient = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    ConcurrencyStamp = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExaminationSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExaminationSessions_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExaminationSessions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExaminationSessions_StaffProfiles_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "StaffProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedicineInventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicineName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    PricePerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicineInventories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    MedicalServiceId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TotalSessions = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentPlans_MedicalServices_MedicalServiceId",
                        column: x => x.MedicalServiceId,
                        principalTable: "MedicalServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TreatmentPlans_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TreatmentPlans_StaffProfiles_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "StaffProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DraftInvoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    ExaminationSessionId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DraftInvoices_ExaminationSessions_ExaminationSessionId",
                        column: x => x.ExaminationSessionId,
                        principalTable: "ExaminationSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DraftInvoices_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    ExaminationSessionId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsAllergyWarningBypassed = table.Column<bool>(type: "bit", nullable: false),
                    AllergyBypassReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prescriptions_ExaminationSessions_ExaminationSessionId",
                        column: x => x.ExaminationSessionId,
                        principalTable: "ExaminationSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prescriptions_StaffProfiles_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "StaffProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentPlanSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TreatmentPlanId = table.Column<int>(type: "int", nullable: false),
                    SessionNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AppointmentId = table.Column<int>(type: "int", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentPlanSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentPlanSessions_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TreatmentPlanSessions_TreatmentPlans_TreatmentPlanId",
                        column: x => x.TreatmentPlanId,
                        principalTable: "TreatmentPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrescriptionId = table.Column<int>(type: "int", nullable: false),
                    MedicineId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrescriptionItems_MedicineInventories_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "MedicineInventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrescriptionItems_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 9, 47, 11, 87, DateTimeKind.Local).AddTicks(7910));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 9, 47, 11, 87, DateTimeKind.Local).AddTicks(9054));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 9, 47, 11, 87, DateTimeKind.Local).AddTicks(9057));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 9, 47, 11, 87, DateTimeKind.Local).AddTicks(9058));

            migrationBuilder.UpdateData(
                table: "MedicalServices",
                keyColumn: "Id",
                keyValue: 1,
                column: "DefaultWarrantyMonths",
                value: null);

            migrationBuilder.UpdateData(
                table: "MedicalServices",
                keyColumn: "Id",
                keyValue: 2,
                column: "DefaultWarrantyMonths",
                value: null);

            migrationBuilder.UpdateData(
                table: "MedicalServices",
                keyColumn: "Id",
                keyValue: 3,
                column: "DefaultWarrantyMonths",
                value: null);

            migrationBuilder.UpdateData(
                table: "MedicalServices",
                keyColumn: "Id",
                keyValue: 4,
                column: "DefaultWarrantyMonths",
                value: 6);

            migrationBuilder.UpdateData(
                table: "MedicalServices",
                keyColumn: "Id",
                keyValue: 5,
                column: "DefaultWarrantyMonths",
                value: 12);

            migrationBuilder.InsertData(
                table: "MedicineInventories",
                columns: new[] { "Id", "MedicineName", "PricePerUnit", "StockQuantity", "Unit" },
                values: new object[,]
                {
                    { 1, "Hapacol 650mg", 2000m, 500, "Viên" },
                    { 2, "Amoxicillin 500mg", 5000m, 300, "Viên" },
                    { 3, "Ibuprofen 400mg", 4000m, 200, "Viên" },
                    { 4, "Paracetamol 500mg", 1000m, 1000, "Viên" },
                    { 5, "Sensodyne Toothpaste", 65000m, 50, "Tuýp" },
                    { 6, "Chlorhexidine Mouthwash", 45000m, 80, "Chai" }
                });

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 9, 47, 11, 87, DateTimeKind.Local).AddTicks(6898));

            migrationBuilder.CreateIndex(
                name: "IX_DentalWarranties_DoctorId",
                table: "DentalWarranties",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DentalWarranties_MedicalServiceId",
                table: "DentalWarranties",
                column: "MedicalServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_DentalWarranties_PatientId",
                table: "DentalWarranties",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_DentalWarranties_WarrantyCode",
                table: "DentalWarranties",
                column: "WarrantyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DraftInvoices_ExaminationSessionId",
                table: "DraftInvoices",
                column: "ExaminationSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftInvoices_PatientId",
                table: "DraftInvoices",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ExaminationSessions_AppointmentId",
                table: "ExaminationSessions",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExaminationSessions_DoctorId",
                table: "ExaminationSessions",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_ExaminationSessions_PatientId",
                table: "ExaminationSessions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_MedicineId",
                table: "PrescriptionItems",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_PrescriptionId",
                table: "PrescriptionItems",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_DoctorId",
                table: "Prescriptions",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_ExaminationSessionId",
                table: "Prescriptions",
                column: "ExaminationSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_PatientId",
                table: "Prescriptions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPlans_DoctorId",
                table: "TreatmentPlans",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPlans_MedicalServiceId",
                table: "TreatmentPlans",
                column: "MedicalServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPlans_PatientId",
                table: "TreatmentPlans",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPlanSessions_AppointmentId",
                table: "TreatmentPlanSessions",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPlanSessions_TreatmentPlanId",
                table: "TreatmentPlanSessions",
                column: "TreatmentPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DentalWarranties");

            migrationBuilder.DropTable(
                name: "DraftInvoices");

            migrationBuilder.DropTable(
                name: "PrescriptionItems");

            migrationBuilder.DropTable(
                name: "TreatmentPlanSessions");

            migrationBuilder.DropTable(
                name: "MedicineInventories");

            migrationBuilder.DropTable(
                name: "Prescriptions");

            migrationBuilder.DropTable(
                name: "TreatmentPlans");

            migrationBuilder.DropTable(
                name: "ExaminationSessions");

            migrationBuilder.DropColumn(
                name: "DefaultWarrantyMonths",
                table: "MedicalServices");

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 3, 23, 2, 29, 601, DateTimeKind.Local).AddTicks(3860));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 3, 23, 2, 29, 601, DateTimeKind.Local).AddTicks(7595));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 3, 23, 2, 29, 601, DateTimeKind.Local).AddTicks(7602));

            migrationBuilder.UpdateData(
                table: "HolidayDates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 3, 23, 2, 29, 601, DateTimeKind.Local).AddTicks(7604));

            migrationBuilder.UpdateData(
                table: "Shifts",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 3, 23, 2, 29, 601, DateTimeKind.Local).AddTicks(468));
        }
    }
}
