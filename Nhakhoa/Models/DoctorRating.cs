using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nhakhoa.Models
{
    public class DoctorRating
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ExaminationSessionId { get; set; }
        [ForeignKey("ExaminationSessionId")]
        public ExaminationSession? ExaminationSession { get; set; }

        [Required]
        public int DoctorId { get; set; } // StaffProfileId
        [ForeignKey("DoctorId")]
        public StaffProfile? Doctor { get; set; }

        [Required]
        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [Required]
        [Range(1, 5)]
        public int Stars { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
