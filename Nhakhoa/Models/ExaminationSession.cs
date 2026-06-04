using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nhakhoa.Models
{
    public class ExaminationSession
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

        public int? AppointmentId { get; set; }
        [ForeignKey("AppointmentId")]
        public Appointment? Appointment { get; set; }

        [Required]
        [MaxLength(500)]
        public string Diagnosis { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? ClinicalNotes { get; set; }

        [MaxLength(2000)]
        public string? TreatmentPlanSummary { get; set; }

        [MaxLength(2000)]
        public string? HomeCareInstructions { get; set; }

        [Required]
        [Column(TypeName = "decimal(3,2)")]
        public decimal PatientCoefficient { get; set; } = 0.00m; // Range: 0.0 - 0.5

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public bool IsCompleted { get; set; } = false;

        [ConcurrencyCheck]
        public Guid ConcurrencyStamp { get; set; } = Guid.NewGuid();
    }
}
