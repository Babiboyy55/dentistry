using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Nhakhoa.Models
{
    public class DentalChair
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClinicId { get; set; }

        [ValidateNever]
        public Clinic? Clinic { get; set; }

        [Required]
        [MaxLength(50)]
        public string ChairCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // "Hoạt động" hoặc "Bảo trì"
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Hoạt động";

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
