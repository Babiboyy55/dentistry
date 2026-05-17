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
            
            // Seed a default admin user (password is "Admin@123" hashed simply for demonstration, in real app use proper BCrypt/Argon2)
            // For now, let's store plain text or simple hash. We will just use plaintext for the immediate mock, or a fake hash.
            // Let's use plaintext "Admin@123" for this simple test if you haven't implemented hashing yet.
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", PasswordHash = "admin", Role = "Admin", IsActive = true, FullName = "Admin System", Email = "admin@clinic.com", PhoneNumber = "0123456789" }
            );
        }
    }
}
