using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nhakhoa.Models
{
    public class Clinic
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string Location { get; set; }

        public int? DefaultSpecialtyId { get; set; }
        public Specialty DefaultSpecialty { get; set; }

        [Required]
        public int Capacity { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Shifts assigned to this clinic
        public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    }
}
