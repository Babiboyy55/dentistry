using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nhakhoa.Models
{
    public class PrescriptionItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PrescriptionId { get; set; }
        [ForeignKey("PrescriptionId")]
        public Prescription? Prescription { get; set; }

        [Required]
        public int MedicineId { get; set; }
        [ForeignKey("MedicineId")]
        public MedicineInventory? Medicine { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [MaxLength(500)]
        public string Dosage { get; set; } = string.Empty; // e.g. "Ngày uống 2 lần, mỗi lần 1 viên"

        [Required]
        public int DurationDays { get; set; }
    }
}
