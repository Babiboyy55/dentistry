using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nhakhoa.Models
{
    public class DentalWarranty
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
        [MaxLength(100)]
        public string WarrantyCode { get; set; } = string.Empty; // Unique code

        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime EndDate { get; set; }

        [MaxLength(1000)]
        public string? Terms { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active, Expired, Voided

        [MaxLength(500)]
        public string? OverrideReason { get; set; } // Set by Admin if overridden
    }
}
