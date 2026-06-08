using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Nhakhoa.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }
        [ValidateNever]
        public Patient? Patient { get; set; }

        [Required]
        public int StaffProfileId { get; set; }
        [ValidateNever]
        public StaffProfile? StaffProfile { get; set; }

        public int? ClinicId { get; set; }
        [ValidateNever]
        public Clinic? Clinic { get; set; }

        public int? DentalChairId { get; set; }
        [ValidateNever]
        public DentalChair? DentalChair { get; set; }

        public int? SpecialtyId { get; set; }
        [ValidateNever]
        public Specialty? Specialty { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        // Giờ hẹn, ví dụ: "08:00"
        [MaxLength(10)]
        public string TimeSlot { get; set; } = string.Empty;

        // "Sáng" hoặc "Chiều"
        [MaxLength(10)]
        public string Session { get; set; } = "Sáng";

        // Trạng thái: "Đã xác nhận", "Đang chờ", "Đang khám", "Đã khám xong", "Đã hủy", "Vắng mặt"
        [MaxLength(30)]
        public string Status { get; set; } = "Đã xác nhận";

        // Walk-in hay đặt trước
        public bool IsWalkIn { get; set; } = false;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Người đặt lịch
        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        // Thời gian thực tế được tiếp nhận vào phòng
        public DateTime? CheckedInAt { get; set; }

        // Thời gian hoàn thành khám
        public DateTime? CompletedAt { get; set; }

        // Số thứ tự trong hàng chờ
        public int? QueueNumber { get; set; }

        [ConcurrencyCheck]
        public Guid ConcurrencyStamp { get; set; } = Guid.NewGuid();
    }
}
