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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.StaffProfile)
                .WithOne(p => p.User)
                .HasForeignKey<StaffProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasOne(u => u.StaffSalaryInfo)
                .WithOne(s => s.User)
                .HasForeignKey<StaffSalaryInfo>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.StaffQualifications)
                .WithOne(q => q.User)
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Seed a default admin user
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", PasswordHash = "admin", Role = "Admin", IsActive = true, FullName = "Admin System", Email = "admin@clinic.com", PhoneNumber = "0123456789", SecurityStamp = "default-admin-security-stamp" }
            );

            // Seed medical services
            modelBuilder.Entity<MedicalService>().HasData(
                new MedicalService { Id = 1, Name = "Khám nội tổng quát", Description = "Khám sàng lọc và tư vấn sức khỏe cơ bản", Price = 500000m, Department = "Nội tổng quát", IsActive = true, UpdatedAt = new DateTime(2023, 10, 12) },
                new MedicalService { Id = 2, Name = "Xét nghiệm công thức máu (24 chỉ số)", Description = "Phân tích huyết học tự động công nghệ cao", Price = 150000m, Department = "Xét nghiệm", IsActive = true, UpdatedAt = new DateTime(2023, 10, 8) },
                new MedicalService { Id = 3, Name = "Siêu âm bụng tổng quát", Description = "Siêu âm 4D ổ bụng và các cơ quan nội tạng", Price = 350000m, Department = "Chẩn đoán hình ảnh", IsActive = false, UpdatedAt = new DateTime(2023, 10, 5) },
                new MedicalService { Id = 4, Name = "Nhổ răng khôn", Description = "Nhổ răng khôn mọc lệch, mọc ngầm sử dụng sóng siêu âm Piezotome", Price = 1200000m, Department = "Nha khoa tổng quát", IsActive = true, UpdatedAt = new DateTime(2023, 10, 10) },
                new MedicalService { Id = 5, Name = "Tẩy trắng răng Laser", Description = "Tẩy trắng răng công nghệ Laser Whitening nhanh chóng, không ê buốt", Price = 2500000m, Department = "Nha khoa thẩm mỹ", IsActive = true, UpdatedAt = new DateTime(2023, 10, 15) }
            );

            // Seed RBAC permission matrix (UC1.5)
            // Modules: key, name, icon, sortOrder
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
            // Default permissions per role
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
