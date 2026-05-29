using System;
using System.ComponentModel.DataAnnotations;

namespace Nhakhoa.Models
{
    public class HolidayDate
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        // "Cố định" hoặc "Đột xuất"
        [MaxLength(20)]
        public string HolidayType { get; set; } = "Cố định";

        // Tự động lặp vào cùng ngày năm sau
        public bool RepeatYearly { get; set; } = false;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Who created this holiday
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
    }
}
