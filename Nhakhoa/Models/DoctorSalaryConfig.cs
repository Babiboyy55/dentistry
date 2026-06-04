using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nhakhoa.Models
{
    /// <summary>
    /// Cấu hình toàn hệ thống cho tính toán lương làm thêm bác sĩ.
    /// Chỉ có một bản ghi (singleton). Id luôn = 1.
    /// </summary>
    public class DoctorSalaryConfig
    {
        [Key]
        public int Id { get; set; } = 1;

        // ── Mức tiền cơ bản theo giờ ──────────────────────
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; } = 210_000m;

        // ── Hệ số học hàm / học vị ────────────────────────
        [Column(TypeName = "decimal(5,2)")]
        public decimal DegreeUniversity { get; set; } = 1.20m;   // Đại học

        [Column(TypeName = "decimal(5,2)")]
        public decimal DegreeMaster { get; set; } = 1.50m;       // Thạc sỹ

        [Column(TypeName = "decimal(5,2)")]
        public decimal DegreeDoctorate { get; set; } = 2.00m;    // Tiến sỹ

        [Column(TypeName = "decimal(5,2)")]
        public decimal DegreeAssocProf { get; set; } = 2.50m;    // Phó giáo sư

        [Column(TypeName = "decimal(5,2)")]
        public decimal DegreeProfessor { get; set; } = 3.00m;    // Giáo sư

        // ── Hệ số ca làm việc theo ngày trong tuần ────────
        [Column(TypeName = "decimal(5,2)")]
        public decimal MultiplierMonday { get; set; } = 1.00m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal MultiplierTuesday { get; set; } = 1.00m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal MultiplierWednesday { get; set; } = 1.00m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal MultiplierThursday { get; set; } = 1.00m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal MultiplierFriday { get; set; } = 1.00m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal MultiplierSaturday { get; set; } = 1.20m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal MultiplierSunday { get; set; } = 1.50m;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
