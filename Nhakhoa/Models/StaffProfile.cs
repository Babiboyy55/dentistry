using System;
using System.ComponentModel.DataAnnotations;

namespace Nhakhoa.Models
{
    public class StaffProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public User User { get; set; }

        [MaxLength(50)]
        public string StaffCode { get; set; }

        [MaxLength(100)]
        public string PositionTitle { get; set; }

        [MaxLength(100)]
        public string Department { get; set; }

        [MaxLength(20)]
        public string Gender { get; set; }

        [MaxLength(200)]
        public string Address { get; set; }

        public DateTime? JoinDate { get; set; }

        [MaxLength(100)]
        public string PrimaryClinic { get; set; }

        public ICollection<DoctorSpecialty> DoctorSpecialties { get; set; } = new List<DoctorSpecialty>();
    }
}
