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
        public DbSet<StaffProfile> StaffProfiles { get; set; }
        public DbSet<StaffSalaryInfo> StaffSalaryInfos { get; set; }
        public DbSet<StaffQualification> StaffQualifications { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<MedicalService> MedicalServices { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<DoctorSpecialty> DoctorSpecialties { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<HolidayDate> HolidayDates { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<ShiftSetting> ShiftSettings { get; set; }

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
                    Id = 201, UserId = 201, StaffCode = "DOC-102", PositionTitle = "Y học cổ truyền", Department = "Nội tổng quát", 
                    Gender = "Nam", Address = "Hà Nội", JoinDate = new DateTime(2022, 5, 10), PrimaryClinic = "Phòng khám A1",
                    DateOfBirth = new DateTime(1980, 5, 12), Cccd = "123456789012", CchnNumber = "CCHN-002341",
                    CchnIssueDate = new DateTime(2015, 6, 1), CchnExpiryDate = new DateTime(2035, 6, 1), CchnProvider = "Sở Y tế Hà Nội",
                    AcademicRank = "Không", AcademicDegree = "Bác sĩ thường", JobRank = "Bác sĩ", ExperienceYears = 8
                },
                new StaffProfile 
                { 
                    Id = 202, UserId = 202, StaffCode = "DOC-205", PositionTitle = "Chẩn đoán hình ảnh", Department = "Chẩn đoán hình ảnh", 
                    Gender = "Nữ", Address = "Đà Nẵng", JoinDate = new DateTime(2021, 8, 15), PrimaryClinic = "Phòng B2",
                    DateOfBirth = new DateTime(1985, 10, 20), Cccd = "234567890123", CchnNumber = "CCHN-009842",
                    CchnIssueDate = new DateTime(2018, 9, 15), CchnExpiryDate = new DateTime(2038, 9, 15), CchnProvider = "Sở Y tế Đà Nẵng",
                    AcademicRank = "Không", AcademicDegree = "Bác sĩ chuyên khoa I", JobRank = "Bác sĩ", ExperienceYears = 5
                },
                new StaffProfile 
                { 
                    Id = 203, UserId = 203, StaffCode = "DOC-098", PositionTitle = "Thần kinh học", Department = "Thần kinh", 
                    Gender = "Nam", Address = "Hồ Chí Minh", JoinDate = new DateTime(2023, 1, 20), PrimaryClinic = "Phòng C1",
                    DateOfBirth = new DateTime(1978, 12, 1), Cccd = "345678901234", CchnNumber = "CCHN-005612",
                    CchnIssueDate = new DateTime(2012, 4, 10), CchnExpiryDate = new DateTime(2032, 4, 10), CchnProvider = "Bộ Y tế",
                    AcademicRank = "Không", AcademicDegree = "Bác sĩ thường", JobRank = "Bác sĩ chính", ExperienceYears = 12
                },
                new StaffProfile 
                { 
                    Id = 204, UserId = 204, StaffCode = "DOC-110", PositionTitle = "Chuyên gia Tim mạch", Department = "Tim mạch", 
                    Gender = "Nữ", Address = "Hà Nội", JoinDate = new DateTime(2023, 4, 1), PrimaryClinic = "Phòng khám A1",
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
                new Specialty { Id = 1, Name = "Tim mạch", Code = "CARD-001", Description = "Chuyên chẩn đoán và điều trị các bệnh lý tim mạch và mạch máu.", UpdatedAt = new DateTime(2023, 10, 10) },
                new Specialty { Id = 2, Name = "Thần kinh", Code = "NEUR-002", Description = "Điều trị các bệnh lý liên quan đến hệ thần kinh trung ương và ngoại biên.", UpdatedAt = new DateTime(2023, 10, 12) },
                new Specialty { Id = 3, Name = "Nhi khoa", Code = "PEDI-003", Description = "Chăm sóc sức khỏe toàn diện, sàng lọc phát triển thể chất ở trẻ em.", UpdatedAt = new DateTime(2023, 10, 15) },
                new Specialty { Id = 4, Name = "Răng Hàm Mặt", Code = "DENT-004", Description = "Điều trị và phục hình răng hàm mặt thẩm mỹ, công nghệ cao.", UpdatedAt = new DateTime(2023, 10, 18) }
            );

            // Seed DoctorSpecialty
            modelBuilder.Entity<DoctorSpecialty>().HasData(
                new DoctorSpecialty { StaffProfileId = 204, SpecialtyId = 1 }, // Sarah Johnson in Cardiovascular
                new DoctorSpecialty { StaffProfileId = 203, SpecialtyId = 2 }  // Le Anh Tuan in Neurology
            );

            // Seed Medical Services (updated with SpecialtyId)
            modelBuilder.Entity<MedicalService>().HasData(
                new MedicalService { Id = 1, Name = "Khám nội tổng quát", Description = "Khám sàng lọc và tư vấn sức khỏe cơ bản", Price = 500000m, Department = "Nội tổng quát", IsActive = true, SpecialtyId = 1, UpdatedAt = new DateTime(2023, 10, 12) },
                new MedicalService { Id = 2, Name = "Xét nghiệm công thức máu (24 chỉ số)", Description = "Phân tích huyết học tự động công nghệ cao", Price = 150000m, Department = "Xét nghiệm", IsActive = true, SpecialtyId = 3, UpdatedAt = new DateTime(2023, 10, 8) },
                new MedicalService { Id = 3, Name = "Siêu âm bụng tổng quát", Description = "Siêu âm 4D ổ bụng và các cơ quan nội tạng", Price = 350000m, Department = "Chẩn đoán hình ảnh", IsActive = false, SpecialtyId = 2, UpdatedAt = new DateTime(2023, 10, 5) },
                new MedicalService { Id = 4, Name = "Nhổ răng khôn", Description = "Nhổ răng khôn mọc lệch, mọc ngầm sử dụng sóng siêu âm Piezotome", Price = 1200000m, Department = "Nha khoa tổng quát", IsActive = true, SpecialtyId = 4, UpdatedAt = new DateTime(2023, 10, 10) },
                new MedicalService { Id = 5, Name = "Tẩy trắng răng Laser", Description = "Tẩy trắng răng công nghệ Laser Whitening nhanh chóng, không ê buốt", Price = 2500000m, Department = "Nha khoa thẩm mỹ", IsActive = true, SpecialtyId = 4, UpdatedAt = new DateTime(2023, 10, 15) }
            );

            // Seed Clinics
            modelBuilder.Entity<Clinic>().HasData(
                new Clinic { Id = 1, Name = "Phòng khám Tim mạch A1", Location = "Tầng 1 - Khu A", DefaultSpecialtyId = 1, Capacity = 15, IsActive = true, UpdatedAt = new DateTime(2023, 10, 10) },
                new Clinic { Id = 2, Name = "Phòng khám Thần kinh B2", Location = "Tầng 2 - Khu B", DefaultSpecialtyId = 2, Capacity = 10, IsActive = true, UpdatedAt = new DateTime(2023, 10, 12) },
                new Clinic { Id = 3, Name = "Phòng khám Răng Hàm Mặt C1", Location = "Tầng 1 - Khu C", DefaultSpecialtyId = 4, Capacity = 20, IsActive = true, UpdatedAt = new DateTime(2023, 10, 15) }
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
        }
    }
}
