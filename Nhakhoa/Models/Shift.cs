using System;
using System.ComponentModel.DataAnnotations;

namespace Nhakhoa.Models
{
    public class Shift
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClinicId { get; set; }
        public Clinic Clinic { get; set; }

        [Required]
        public int StaffProfileId { get; set; }
        public StaffProfile StaffProfile { get; set; }

        [Required]
        public DateTime ShiftDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
