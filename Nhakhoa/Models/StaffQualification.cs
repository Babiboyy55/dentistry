using System.ComponentModel.DataAnnotations;

namespace Nhakhoa.Models
{
    public class StaffQualification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public User User { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(150)]
        public string Major { get; set; }

        [MaxLength(150)]
        public string Institution { get; set; }

        public int? Year { get; set; }

        [Required]
        [MaxLength(30)]
        public string Category { get; set; } // Degree, Certificate

        [MaxLength(255)]
        public string? ImagePath { get; set; }

        [MaxLength(50)]
        public string? AcademicRank { get; set; }

        [MaxLength(50)]
        public string? AcademicDegree { get; set; }
    }
}
