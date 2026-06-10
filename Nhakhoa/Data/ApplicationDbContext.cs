using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Nhakhoa.Models;

namespace Nhakhoa.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<PatientAccount> PatientAccounts { get; set; }
        public DbSet<StaffProfile> StaffProfiles { get; set; }
        public DbSet<StaffSalaryInfo> StaffSalaryInfos { get; set; }
        public DbSet<StaffQualification> StaffQualifications { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<MedicalService> MedicalServices { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<DentalChair> DentalChairs { get; set; }
        public DbSet<DoctorSpecialty> DoctorSpecialties { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<HolidayDate> HolidayDates { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<ShiftSetting> ShiftSettings { get; set; }
        public DbSet<PatientToothRecord> PatientToothRecords { get; set; }
        public DbSet<ExaminationSession> ExaminationSessions { get; set; }
        public DbSet<TreatmentPlan> TreatmentPlans { get; set; }
        public DbSet<TreatmentPlanSession> TreatmentPlanSessions { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
        public DbSet<MedicineInventory> MedicineInventories { get; set; }
        public DbSet<DentalWarranty> DentalWarranties { get; set; }
        public DbSet<DraftInvoice> DraftInvoices { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<DoctorSalaryConfig> DoctorSalaryConfigs { get; set; }
        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<RefundApproval> RefundApprovals { get; set; }
        public DbSet<DailyReconciliation> DailyReconciliations { get; set; }
        public DbSet<ReconciliationDetail> ReconciliationDetails { get; set; }
        public DbSet<DoctorRating> DoctorRatings { get; set; }
        public DbSet<MedicineTransaction> MedicineTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User - StaffProfile One-to-One
            modelBuilder.Entity<User>()
                .HasOne(u => u.StaffProfile)
                .WithOne(p => p.User)
                .HasForeignKey<StaffProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - StaffSalaryInfo One-to-One
            modelBuilder.Entity<User>()
                .HasOne(u => u.StaffSalaryInfo)
                .WithOne(s => s.User)
                .HasForeignKey<StaffSalaryInfo>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User - StaffQualification One-to-Many
            modelBuilder.Entity<User>()
                .HasMany(u => u.StaffQualifications)
                .WithOne(q => q.User)
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // DoctorSpecialty Composite Key
            modelBuilder.Entity<DoctorSpecialty>()
                .HasKey(ds => new { ds.StaffProfileId, ds.SpecialtyId });

            modelBuilder.Entity<DoctorSpecialty>()
                .HasOne(ds => ds.StaffProfile)
                .WithMany(sp => sp.DoctorSpecialties)
                .HasForeignKey(ds => ds.StaffProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorSpecialty>()
                .HasOne(ds => ds.Specialty)
                .WithMany(s => s.DoctorSpecialties)
                .HasForeignKey(ds => ds.SpecialtyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Clinic - Specialty (DefaultSpecialty) One-to-Many
            modelBuilder.Entity<Clinic>()
                .HasOne(c => c.DefaultSpecialty)
                .WithMany(s => s.Clinics)
                .HasForeignKey(c => c.DefaultSpecialtyId)
                .OnDelete(DeleteBehavior.SetNull);

            // Clinic - DentalChairs One-to-Many
            modelBuilder.Entity<DentalChair>()
                .HasOne(dc => dc.Clinic)
                .WithMany(c => c.DentalChairs)
                .HasForeignKey(dc => dc.ClinicId)
                .OnDelete(DeleteBehavior.Cascade);

            // Shift relationships
            modelBuilder.Entity<Shift>()
                .HasOne(s => s.Clinic)
                .WithMany(c => c.Shifts)
                .HasForeignKey(s => s.ClinicId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Shift>()
                .HasOne(s => s.StaffProfile)
                .WithMany()
                .HasForeignKey(s => s.StaffProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Shift>()
                .HasOne(s => s.DentalChair)
                .WithMany()
                .HasForeignKey(s => s.DentalChairId)
                .OnDelete(DeleteBehavior.Restrict);

            // MedicalService - Specialty
            modelBuilder.Entity<MedicalService>()
                .HasOne(ms => ms.Specialty)
                .WithMany(s => s.MedicalServices)
                .HasForeignKey(ms => ms.SpecialtyId)
                .OnDelete(DeleteBehavior.SetNull);

            // Appointment - Patient
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Appointment - StaffProfile (no cascade to avoid multiple cascade paths)
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.StaffProfile)
                .WithMany()
                .HasForeignKey(a => a.StaffProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment - Clinic
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Clinic)
                .WithMany()
                .HasForeignKey(a => a.ClinicId)
                .OnDelete(DeleteBehavior.SetNull);

            // Appointment - Specialty
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Specialty)
                .WithMany()
                .HasForeignKey(a => a.SpecialtyId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.DentalChair)
                .WithMany()
                .HasForeignKey(a => a.DentalChairId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => new { a.StaffProfileId, a.AppointmentDate, a.TimeSlot })
                .IsUnique()
                .HasFilter("[Status] != 'Đã hủy'");

            // Patient - PrimaryDoctor relationship
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.PrimaryDoctor)
                .WithMany()
                .HasForeignKey(p => p.PrimaryDoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            // PatientToothRecord relationships
            modelBuilder.Entity<PatientToothRecord>()
                .HasOne(tr => tr.Patient)
                .WithMany(p => p.ToothRecords)
                .HasForeignKey(tr => tr.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PatientToothRecord>()
                .HasOne(tr => tr.Doctor)
                .WithMany()
                .HasForeignKey(tr => tr.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PatientToothRecord>()
                .HasOne(tr => tr.Appointment)
                .WithMany()
                .HasForeignKey(tr => tr.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExaminationSession relationships
            modelBuilder.Entity<ExaminationSession>()
                .HasOne(es => es.Patient)
                .WithMany()
                .HasForeignKey(es => es.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExaminationSession>()
                .HasOne(es => es.Doctor)
                .WithMany()
                .HasForeignKey(es => es.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExaminationSession>()
                .HasOne(es => es.Appointment)
                .WithMany()
                .HasForeignKey(es => es.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // TreatmentPlan relationships
            modelBuilder.Entity<TreatmentPlan>()
                .HasOne(tp => tp.Patient)
                .WithMany()
                .HasForeignKey(tp => tp.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TreatmentPlan>()
                .HasOne(tp => tp.Doctor)
                .WithMany()
                .HasForeignKey(tp => tp.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TreatmentPlan>()
                .HasOne(tp => tp.MedicalService)
                .WithMany()
                .HasForeignKey(tp => tp.MedicalServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // TreatmentPlanSession relationships
            modelBuilder.Entity<TreatmentPlanSession>()
                .HasOne(tps => tps.TreatmentPlan)
                .WithMany(tp => tp.Sessions)
                .HasForeignKey(tps => tps.TreatmentPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TreatmentPlanSession>()
                .HasOne(tps => tps.Appointment)
                .WithMany()
                .HasForeignKey(tps => tps.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prescription relationships
            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Patient)
                .WithMany()
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Doctor)
                .WithMany()
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.ExaminationSession)
                .WithMany()
                .HasForeignKey(p => p.ExaminationSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // PrescriptionItem relationships
            modelBuilder.Entity<PrescriptionItem>()
                .HasOne(pi => pi.Prescription)
                .WithMany(p => p.Items)
                .HasForeignKey(pi => pi.PrescriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PrescriptionItem>()
                .HasOne(pi => pi.Medicine)
                .WithMany()
                .HasForeignKey(pi => pi.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            // DentalWarranty relationships
            modelBuilder.Entity<DentalWarranty>()
                .HasOne(dw => dw.Patient)
                .WithMany()
                .HasForeignKey(dw => dw.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DentalWarranty>()
                .HasOne(dw => dw.Doctor)
                .WithMany()
                .HasForeignKey(dw => dw.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DentalWarranty>()
                .HasOne(dw => dw.MedicalService)
                .WithMany()
                .HasForeignKey(dw => dw.MedicalServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DentalWarranty>()
                .HasIndex(dw => dw.WarrantyCode)
                .IsUnique();

            // DraftInvoice relationships
            modelBuilder.Entity<DraftInvoice>()
                .HasOne(di => di.Patient)
                .WithMany()
                .HasForeignKey(di => di.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DraftInvoice>()
                .HasOne(di => di.ExaminationSession)
                .WithMany()
                .HasForeignKey(di => di.ExaminationSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed Users (Admin & 4 Doctors)
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", PasswordHash = "admin", Role = "Admin", IsActive = true, FullName = "Admin System", Email = "admin@clinic.com", PhoneNumber = "0123456789", SecurityStamp = "default-admin-security-stamp" },
                new User { Id = 201, Username = "doctor1", PasswordHash = "123456", Role = "Doctor", IsActive = true, FullName = "BS. Nguyễn Văn Đạt", Email = "dat.nguyen@clinic.com", PhoneNumber = "0987654321", SecurityStamp = "doctor-1-security-stamp" },
                new User { Id = 202, Username = "doctor2", PasswordHash = "123456", Role = "Doctor", IsActive = false, FullName = "BS. Trần Thanh Mai", Email = "mai.tran@clinic.com", PhoneNumber = "0987654322", SecurityStamp = "doctor-2-security-stamp" },
                new User { Id = 203, Username = "doctor3", PasswordHash = "123456", Role = "Doctor", IsActive = true, FullName = "BS. Lê Anh Tuấn", Email = "tuan.le@clinic.com", PhoneNumber = "0987654323", SecurityStamp = "doctor-3-security-stamp" },
                new User { Id = 204, Username = "doctor4", PasswordHash = "123456", Role = "Doctor", IsActive = true, FullName = "BS. Sarah Johnson", Email = "sarah.johnson@clinic.com", PhoneNumber = "0987654324", SecurityStamp = "doctor-4-security-stamp" }
            );

            // Seed StaffProfiles
            modelBuilder.Entity<StaffProfile>().HasData(
                new StaffProfile 
                { 
                    Id = 201, UserId = 201, StaffCode = "DOC-102", PositionTitle = "Nha sĩ Tổng quát", Department = "Khoa khám bệnh", 
                    Gender = "Nam", Address = "Hà Nội", JoinDate = new DateTime(2022, 5, 10), PrimaryClinic = "Phòng khám Nha khoa tổng quát A1",
                    DateOfBirth = new DateTime(1980, 5, 12), Cccd = "123456789012", CchnNumber = "CCHN-002341",
                    CchnIssueDate = new DateTime(2015, 6, 1), CchnExpiryDate = new DateTime(2035, 6, 1), CchnProvider = "Sở Y tế Hà Nội",
                    AcademicRank = "Không", AcademicDegree = "Bác sĩ thường", JobRank = "Bác sĩ", ExperienceYears = 8
                },
                new StaffProfile 
                { 
                    Id = 202, UserId = 202, StaffCode = "DOC-205", PositionTitle = "Chuyên gia Phục hình răng", Department = "Khoa thẩm mỹ", 
                    Gender = "Nữ", Address = "Đà Nẵng", JoinDate = new DateTime(2021, 8, 15), PrimaryClinic = "Phòng khám Thẩm mỹ & Chỉnh nha B2",
                    DateOfBirth = new DateTime(1985, 10, 20), Cccd = "234567890123", CchnNumber = "CCHN-009842",
                    CchnIssueDate = new DateTime(2018, 9, 15), CchnExpiryDate = new DateTime(2038, 9, 15), CchnProvider = "Sở Y tế Đà Nẵng",
                    AcademicRank = "Không", AcademicDegree = "Bác sĩ chuyên khoa I", JobRank = "Bác sĩ", ExperienceYears = 5
                },
                new StaffProfile 
                { 
                    Id = 203, UserId = 203, StaffCode = "DOC-098", PositionTitle = "Chuyên gia Chỉnh nha", Department = "Khoa chỉnh nha", 
                    Gender = "Nam", Address = "Hồ Chí Minh", JoinDate = new DateTime(2023, 1, 20), PrimaryClinic = "Phòng khám Thẩm mỹ & Chỉnh nha B2",
                    DateOfBirth = new DateTime(1978, 12, 1), Cccd = "345678901234", CchnNumber = "CCHN-005612",
                    CchnIssueDate = new DateTime(2012, 4, 10), CchnExpiryDate = new DateTime(2032, 4, 10), CchnProvider = "Bộ Y tế",
                    AcademicRank = "Không", AcademicDegree = "Bác sĩ thường", JobRank = "Bác sĩ chính", ExperienceYears = 12
                },
                new StaffProfile 
                { 
                    Id = 204, UserId = 204, StaffCode = "DOC-110", PositionTitle = "Chuyên gia Cấy ghép Implant", Department = "Khoa cấy ghép", 
                    Gender = "Nữ", Address = "Hà Nội", JoinDate = new DateTime(2023, 4, 1), PrimaryClinic = "Phòng khám Cấy ghép Implant C1",
                    DateOfBirth = new DateTime(1988, 3, 15), Cccd = "001085002931", CchnNumber = "CCHN-007788",
                    CchnIssueDate = new DateTime(2016, 6, 8), CchnExpiryDate = new DateTime(2026, 6, 8), CchnProvider = "Sở Y tế Hà Nội",
                    AcademicRank = "Không", AcademicDegree = "Bác sĩ chuyên khoa II", JobRank = "Bác sĩ cao cấp", ExperienceYears = 15
                }
            );


            // Seed StaffSalaryInfos
            modelBuilder.Entity<StaffSalaryInfo>().HasData(
                new StaffSalaryInfo
                {
                    Id = 1,
                    UserId = 201,
                    BaseSalary = 12000000m,
                    DegreeMultiplier = 1.00m,
                    DegreeTitle = "Bác sĩ thường",
                    RankMultiplier = 1.00m,
                    RankTitle = "Bác sĩ",
                    SpecializationAllowance = 2500000m,
                    SeniorityAllowance = 1500000m,
                    MonthlyBonus = 1200000m,
                    OtherDeductions = 100000m,
                    IsRankChangePending = false
                },
                new StaffSalaryInfo
                {
                    Id = 2,
                    UserId = 202,
                    BaseSalary = 15000000m,
                    DegreeMultiplier = 1.30m,
                    DegreeTitle = "Bác sĩ chuyên khoa I",
                    RankMultiplier = 1.00m,
                    RankTitle = "Bác sĩ",
                    SpecializationAllowance = 3500000m,
                    SeniorityAllowance = 2000000m,
                    MonthlyBonus = 2500000m,
                    OtherDeductions = 0m,
                    IsRankChangePending = false
                },
                new StaffSalaryInfo
                {
                    Id = 3,
                    UserId = 203,
                    BaseSalary = 18000000m,
                    DegreeMultiplier = 1.00m,
                    DegreeTitle = "Bác sĩ thường",
                    RankMultiplier = 1.20m,
                    RankTitle = "Bác sĩ chính",
                    SpecializationAllowance = 4000000m,
                    SeniorityAllowance = 3500000m,
                    MonthlyBonus = 3000000m,
                    OtherDeductions = 200000m,
                    IsRankChangePending = false
                },
                new StaffSalaryInfo
                {
                    Id = 4,
                    UserId = 204,
                    BaseSalary = 25000000m,
                    DegreeMultiplier = 1.40m,
                    DegreeTitle = "Bác sĩ chuyên khoa II",
                    RankMultiplier = 1.30m,
                    RankTitle = "Bác sĩ cao cấp",
                    SpecializationAllowance = 5000000m,
                    SeniorityAllowance = 5500000m,
                    MonthlyBonus = 4500000m,
                    OtherDeductions = 0m,
                    IsRankChangePending = false
                }
            );

            // Seed Specialties
            modelBuilder.Entity<Specialty>().HasData(
                new Specialty { Id = 1, Name = "Nha khoa tổng quát", Code = "NKTQ", Description = "Khám răng tổng quát, nhổ răng, chữa tủy và điều trị các bệnh lý răng miệng cơ bản.", UpdatedAt = new DateTime(2023, 10, 10) },
                new Specialty { Id = 2, Name = "Răng sứ thẩm mỹ", Code = "RSTM", Description = "Phục hình răng sứ thẩm mỹ, dán sứ Veneer siêu mỏng và tẩy trắng răng.", UpdatedAt = new DateTime(2023, 10, 12) },
                new Specialty { Id = 3, Name = "Chỉnh nha - Niềng răng", Code = "CNNR", Description = "Nắn chỉnh răng lệch lạc, răng thưa, hô, móm bằng khay trong suốt hoặc mắc cài.", UpdatedAt = new DateTime(2023, 10, 15) },
                new Specialty { Id = 4, Name = "Cấy ghép Implant", Code = "IMPL", Description = "Phục hình răng đã mất bằng chân răng nhân tạo Implant công nghệ hiện đại.", UpdatedAt = new DateTime(2023, 10, 18) }
            );

            // Seed DoctorSpecialty
            modelBuilder.Entity<DoctorSpecialty>().HasData(
                new DoctorSpecialty { StaffProfileId = 204, SpecialtyId = 4 }, // Sarah Johnson in Implantology
                new DoctorSpecialty { StaffProfileId = 203, SpecialtyId = 3 }  // Le Anh Tuan in Orthodontics
            );

            // Seed Medical Services (updated with SpecialtyId)
            modelBuilder.Entity<MedicalService>().HasData(
                new MedicalService { Id = 1, Name = "Khám & Tư vấn răng miệng", Description = "Khám răng tổng quát, chụp phim X-quang răng và lên phác đồ điều trị.", Price = 100000m, Department = "Khám bệnh", IsActive = true, SpecialtyId = 1, DefaultWarrantyMonths = null, UpdatedAt = new DateTime(2023, 10, 12) },
                new MedicalService { Id = 2, Name = "Niềng răng mắc cài kim loại", Description = "Điều chỉnh khớp cắn bằng hệ thống mắc cài kim loại cao cấp.", Price = 25000000m, Department = "Chỉnh nha", IsActive = true, SpecialtyId = 3, DefaultWarrantyMonths = null, UpdatedAt = new DateTime(2023, 10, 8) },
                new MedicalService { Id = 3, Name = "Răng sứ Cercon HT", Description = "Phục hình răng sứt mẻ, ố vàng bằng răng toàn sứ Cercon nhập khẩu Đức.", Price = 5000000m, Department = "Nha khoa thẩm mỹ", IsActive = true, SpecialtyId = 2, DefaultWarrantyMonths = null, UpdatedAt = new DateTime(2023, 10, 5) },
                new MedicalService { Id = 4, Name = "Nhổ răng khôn Piezotome", Description = "Nhổ răng khôn mọc ngầm, lệch bằng máy siêu âm Piezotome không đau, mau lành.", Price = 1500000m, Department = "Tiểu phẫu", IsActive = true, SpecialtyId = 1, DefaultWarrantyMonths = 6, UpdatedAt = new DateTime(2023, 10, 10) },
                new MedicalService { Id = 5, Name = "Cấy ghép Implant Dentium", Description = "Phục hình răng đã mất bằng chân răng nhân tạo Implant Dentium.", Price = 18000000m, Department = "Cấy ghép răng", IsActive = true, SpecialtyId = 4, DefaultWarrantyMonths = 120, UpdatedAt = new DateTime(2023, 10, 15) }
            );

            // Seed MedicineInventory
            modelBuilder.Entity<MedicineInventory>().HasData(
                new MedicineInventory { Id = 1, MedicineName = "Hapacol 650mg", StockQuantity = 500, PricePerUnit = 2000m, Unit = "Viên" },
                new MedicineInventory { Id = 2, MedicineName = "Amoxicillin 500mg", StockQuantity = 300, PricePerUnit = 5000m, Unit = "Viên" },
                new MedicineInventory { Id = 3, MedicineName = "Ibuprofen 400mg", StockQuantity = 200, PricePerUnit = 4000m, Unit = "Viên" },
                new MedicineInventory { Id = 4, MedicineName = "Paracetamol 500mg", StockQuantity = 1000, PricePerUnit = 1000m, Unit = "Viên" },
                new MedicineInventory { Id = 5, MedicineName = "Sensodyne Toothpaste", StockQuantity = 50, PricePerUnit = 65000m, Unit = "Tuýp" },
                new MedicineInventory { Id = 6, MedicineName = "Chlorhexidine Mouthwash", StockQuantity = 80, PricePerUnit = 45000m, Unit = "Chai" }
            );

            // Seed Clinics
            modelBuilder.Entity<Clinic>().HasData(
                new Clinic { Id = 1, Name = "Phòng khám Nha khoa tổng quát A1", Location = "Tầng 1 - Khu A", DefaultSpecialtyId = 1, Capacity = 15, IsActive = true, UpdatedAt = new DateTime(2023, 10, 10) },
                new Clinic { Id = 2, Name = "Phòng khám Thẩm mỹ & Chỉnh nha B2", Location = "Tầng 2 - Khu B", DefaultSpecialtyId = 2, Capacity = 10, IsActive = true, UpdatedAt = new DateTime(2023, 10, 12) },
                new Clinic { Id = 3, Name = "Phòng khám Cấy ghép Implant C1", Location = "Tầng 1 - Khu C", DefaultSpecialtyId = 4, Capacity = 20, IsActive = true, UpdatedAt = new DateTime(2023, 10, 15) }
            );


            // Seed Shifts (Clinic 1 has a future shift with StaffProfileId 204)
            modelBuilder.Entity<Shift>().HasData(
                new Shift { Id = 1, ClinicId = 1, StaffProfileId = 204, ShiftDate = new DateTime(2026, 12, 10), IsActive = true }
            );

            // Seed Vietnamese Public Holidays
            modelBuilder.Entity<HolidayDate>().HasData(
                new HolidayDate { Id = 1, Name = "Tết Dương Lịch", Date = new DateTime(2026, 1, 1), HolidayType = "Cố định", RepeatYearly = true, Notes = "Nghỉ Tết Dương Lịch hàng năm", CreatedBy = "Hệ thống" },
                new HolidayDate { Id = 2, Name = "Ngày Giải phóng Miền Nam", Date = new DateTime(2026, 4, 30), HolidayType = "Cố định", RepeatYearly = true, Notes = "Kỷ niệm Ngày Giải phóng Miền Nam 30/4", CreatedBy = "Hệ thống" },
                new HolidayDate { Id = 3, Name = "Ngày Quốc tế Lao động", Date = new DateTime(2026, 5, 1), HolidayType = "Cố định", RepeatYearly = true, Notes = "Ngày Quốc tế Lao động 1/5", CreatedBy = "Hệ thống" },
                new HolidayDate { Id = 4, Name = "Ngày Quốc Khánh", Date = new DateTime(2026, 9, 2), HolidayType = "Cố định", RepeatYearly = true, Notes = "Ngày Quốc Khánh Việt Nam 2/9", CreatedBy = "Hệ thống" }
            );

            // Seed ShiftSettings
            modelBuilder.Entity<ShiftSetting>().HasData(
                new ShiftSetting { Id = 1, ShiftName = "Sáng", StartTime = "07:00", EndTime = "12:00", DurationHours = 5.0, MaxShiftsPerWeek = 6 },
                new ShiftSetting { Id = 2, ShiftName = "Chiều", StartTime = "13:00", EndTime = "17:00", DurationHours = 4.0, MaxShiftsPerWeek = 6 }
            );


            // Seed RBAC permission matrix (UC1.5)
            var modules = new[]
            {
                ("account_rbac",   "Quản lý tài khoản & RBAC",           "bi-person-lock",   1),
                ("audit_log",      "Audit log & báo cáo hệ thống",          "bi-journal-check", 2),
                ("system_config",  "Cấu hình hệ thống",                    "bi-gear",          3),
                ("emr",            "Bệnh án điện tử (EMR)",               "bi-file-medical",  4),
                ("prescription",   "Kê đơn thuốc",                          "bi-capsule",       5),
                ("lab_test",       "Chỉ định & kết quả xét nghiệm",      "bi-eyedropper",    6),
                ("schedule_view",  "Lịch khám cá nhân (read)",            "bi-calendar-event",7),
                ("appointment",    "Quản lý lịch hẹn",                     "bi-calendar-check",8),
                ("patient_reg",    "Đăng ký / tìm kiếm bệnh nhân",       "bi-person-plus",   9),
                ("invoice",        "Tạo hóa đơn & thu tiền",              "bi-receipt",      10),
                ("patient_admin",  "Thông tin hành chính bệnh nhân",      "bi-clipboard2-data",11),
            };

            var defaultPerms = new Dictionary<string, HashSet<string>>
            {
                ["Admin"]        = new(){ "account_rbac","audit_log","system_config","emr","prescription","lab_test","schedule_view","appointment","patient_reg","invoice","patient_admin" },
                ["Doctor"]       = new(){ "emr","prescription","lab_test","schedule_view" },
                ["Receptionist"] = new(){ "appointment","patient_reg","invoice","patient_admin" },
            };

            var roles = new[] { "Admin", "Doctor", "Receptionist" };
            int seedId = 1;
            foreach (var role in roles)
            {
                foreach (var (key, name, icon, sort) in modules)
                {
                    bool allowed = defaultPerms[role].Contains(key);
                    modelBuilder.Entity<RolePermission>().HasData(
                        new RolePermission { Id = seedId++, Role = role, ModuleKey = key, ModuleName = name, ModuleIcon = icon, SortOrder = sort, IsAllowed = allowed }
                    );
                }
            }

            // Seed default DoctorSalaryConfig (singleton, Id=1)
            modelBuilder.Entity<DoctorSalaryConfig>().HasData(new DoctorSalaryConfig
            {
                Id = 1,
                HourlyRate = 210_000m,
                DegreeUniversity = 1.20m,
                DegreeMaster = 1.50m,
                DegreeDoctorate = 2.00m,
                DegreeAssocProf = 2.50m,
                DegreeProfessor = 3.00m,
                MultiplierMonday = 1.00m,
                MultiplierTuesday = 1.00m,
                MultiplierWednesday = 1.00m,
                MultiplierThursday = 1.00m,
                MultiplierFriday = 1.00m,
                MultiplierSaturday = 1.20m,
                MultiplierSunday = 1.50m,
                UpdatedAt = new DateTime(2026, 1, 1)
            });

            // Invoice configurations
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Patient)
                .WithMany()
                .HasForeignKey(i => i.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.ExaminationSession)
                .WithMany()
                .HasForeignKey(i => i.ExaminationSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            // InvoiceDetail relationships
            modelBuilder.Entity<InvoiceDetail>()
                .HasOne(id => id.Invoice)
                .WithMany(i => i.InvoiceDetails)
                .HasForeignKey(id => id.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InvoiceDetail>()
                .HasOne(id => id.MedicalService)
                .WithMany()
                .HasForeignKey(id => id.MedicalServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment relationships
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Refund relationships
            modelBuilder.Entity<Refund>()
                .HasOne(r => r.Invoice)
                .WithMany(i => i.Refunds)
                .HasForeignKey(r => r.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // RefundApproval relationships
            modelBuilder.Entity<RefundApproval>()
                .HasOne(ra => ra.Refund)
                .WithMany(r => r.ApprovalHistory)
                .HasForeignKey(ra => ra.RefundId)
                .OnDelete(DeleteBehavior.Cascade);

            // DailyReconciliation configurations
            modelBuilder.Entity<DailyReconciliation>()
                .HasIndex(dr => dr.ReconciliationDate)
                .IsUnique();

            // ReconciliationDetail relationships
            modelBuilder.Entity<ReconciliationDetail>()
                .HasOne(rd => rd.DailyReconciliation)
                .WithMany(dr => dr.Details)
                .HasForeignKey(rd => rd.DailyReconciliationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed default PaymentMethods
            modelBuilder.Entity<PaymentMethod>().HasData(
                new PaymentMethod { Id = 1, Name = "Tiền mặt", Code = "CASH", IsEnabled = true, IsDigitalGateway = false, UpdatedAt = new DateTime(2026, 1, 1) },
                new PaymentMethod { Id = 2, Name = "Chuyển khoản", Code = "BANK", IsEnabled = true, IsDigitalGateway = false, UpdatedAt = new DateTime(2026, 1, 1) },
                new PaymentMethod { Id = 3, Name = "VNPay", Code = "VNPAY", IsEnabled = false, IsDigitalGateway = true, Environment = "Sandbox", EndpointUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html", UpdatedAt = new DateTime(2026, 1, 1) },
                new PaymentMethod { Id = 4, Name = "MoMo", Code = "MOMO", IsEnabled = false, IsDigitalGateway = true, Environment = "Sandbox", EndpointUrl = "https://test-payment.momo.vn/v2/gateway/api/create", UpdatedAt = new DateTime(2026, 1, 1) },
                new PaymentMethod { Id = 5, Name = "Bảo hiểm y tế", Code = "INSURANCE", IsEnabled = true, IsDigitalGateway = false, UpdatedAt = new DateTime(2026, 1, 1) }
            );

            modelBuilder.Entity<Invoice>().HasData(
                new Invoice { Id = 1, InvoiceCode = "HD-CASH-TEST", PatientId = null, SubTotal = 0m, VATPercent = 10m, VATAmount = 0m, DiscountAmount = 0m, TotalAmount = 500000m, PaymentMethodCode = "CASH", Status = "Chờ thanh toán", Notes = "Hóa đơn thử nghiệm tiền mặt", CreatedBy = "admin", IssuedAt = new DateTime(2026, 1, 1) }
            );

            // DoctorRating configuration
            modelBuilder.Entity<DoctorRating>()
                .HasOne(dr => dr.ExaminationSession)
                .WithMany()
                .HasForeignKey(dr => dr.ExaminationSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorRating>()
                .HasOne(dr => dr.Doctor)
                .WithMany()
                .HasForeignKey(dr => dr.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DoctorRating>()
                .HasOne(dr => dr.Patient)
                .WithMany()
                .HasForeignKey(dr => dr.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PatientAccount>()
                .HasIndex(pa => pa.PhoneNumber)
                .IsUnique();

            // MedicineTransaction relationships
            modelBuilder.Entity<MedicineTransaction>()
                .HasOne(mt => mt.Medicine)
                .WithMany()
                .HasForeignKey(mt => mt.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
