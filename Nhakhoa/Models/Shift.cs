using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Nhakhoa.Models
{
    public class Shift
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClinicId { get; set; }
        public Clinic Clinic { get; set; } = null!;

        [Required]
        public int StaffProfileId { get; set; }
        public StaffProfile StaffProfile { get; set; } = null!;

        public int? DentalChairId { get; set; }
        [ValidateNever]
        public DentalChair? DentalChair { get; set; }

        [Required]
        public DateTime ShiftDate { get; set; }

        // "Sáng" (07:00-12:00) hoặc "Chiều" (13:00-17:00)
        [Required, MaxLength(10)]
        public string ShiftType { get; set; } = "Sáng";

        public bool IsActive { get; set; } = true;

        // Who registered this shift (doctor self or admin on behalf)
        [MaxLength(100)]
        public string? RegisteredBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
