using System.ComponentModel.DataAnnotations;

namespace Nhakhoa.Models
{
    public class DoctorSpecialty
    {
        public int StaffProfileId { get; set; }
        public StaffProfile StaffProfile { get; set; }

        public int SpecialtyId { get; set; }
        public Specialty Specialty { get; set; }
    }
}
