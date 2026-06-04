using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nhakhoa.Models
{
    public class Prescription
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

        public int? ExaminationSessionId { get; set; }
        [ForeignKey("ExaminationSessionId")]
        public ExaminationSession? ExaminationSession { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Prescribed"; // Prescribed, Dispensed

        [Required]
        public bool IsAllergyWarningBypassed { get; set; } = false;

        [MaxLength(500)]
        public string? AllergyBypassReason { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
    }
}
