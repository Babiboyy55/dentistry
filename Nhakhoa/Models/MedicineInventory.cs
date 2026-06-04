using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nhakhoa.Models
{
    public class MedicineInventory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string MedicineName { get; set; } = string.Empty;

        [Required]
        public int StockQuantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerUnit { get; set; }

        [Required]
        [MaxLength(100)]
        public string Unit { get; set; } = "Viên"; // e.g. Viên, Chai, Tuýp
    }
}
