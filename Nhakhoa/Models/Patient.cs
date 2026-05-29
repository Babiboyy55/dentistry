using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nhakhoa.Models
{
    public class Patient
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Email { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(500)]
        public string? AllergyHistory { get; set; }

        // Unique patient code e.g. BN-00001
        [MaxLength(20)]
        public string PatientCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
