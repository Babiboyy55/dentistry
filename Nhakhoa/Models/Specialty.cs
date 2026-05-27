using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nhakhoa.Models
{
    public class Specialty
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Relationships
        public ICollection<DoctorSpecialty> DoctorSpecialties { get; set; } = new List<DoctorSpecialty>();
        public ICollection<MedicalService> MedicalServices { get; set; } = new List<MedicalService>();
        public ICollection<Clinic> Clinics { get; set; } = new List<Clinic>();
    }
}
