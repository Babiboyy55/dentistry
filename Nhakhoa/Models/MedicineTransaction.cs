using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nhakhoa.Models
{
    public class MedicineTransaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MedicineId { get; set; }
        [ForeignKey("MedicineId")]
        public MedicineInventory? Medicine { get; set; }

        /// <summary>Nhập kho | Xuất kho | Điều chỉnh</summary>
        [Required]
        [MaxLength(50)]
        public string TransactionType { get; set; } = "Nhập kho";

        /// <summary>Số lượng giao dịch (luôn dương, ý nghĩa do TransactionType quyết định)</summary>
        [Required]
        public int Quantity { get; set; }

        public int StockBefore { get; set; }
        public int StockAfter { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        [MaxLength(200)]
        public string? CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>Nếu xuất kho theo đơn thuốc, lưu PrescriptionId</summary>
        public int? PrescriptionId { get; set; }
    }
}
