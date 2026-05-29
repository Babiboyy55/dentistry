using System.ComponentModel.DataAnnotations;

namespace Nhakhoa.Models
{
    public class ShiftSetting
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string ShiftName { get; set; } = string.Empty; // Sáng / Chiều

        [Required, MaxLength(10)]
        public string StartTime { get; set; } = "07:00";

        [Required, MaxLength(10)]
        public string EndTime { get; set; } = "12:00";

        public double DurationHours { get; set; } = 5.0;

        // Tối đa số ca một nhân sự được đăng ký trong tuần
        public int MaxShiftsPerWeek { get; set; } = 6;
    }
}
