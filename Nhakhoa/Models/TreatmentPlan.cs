using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nhakhoa.Models
{
    public class TreatmentPlan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [Required]
        public int DoctorId { get; set; } // StaffProfileId
        [ForeignKey("DoctorId")]
        public StaffProfile? Doctor { get; set; }

        [Required]
        public int MedicalServiceId { get; set; }
        [ForeignKey("MedicalServiceId")]
        public MedicalService? MedicalService { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public int TotalSessions { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active, Completed, Cancelled

        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<TreatmentPlanSession> Sessions { get; set; } = new List<TreatmentPlanSession>();
    }
}
