using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Nhakhoa.Models
{
    public class ReconciliationDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DailyReconciliationId { get; set; }
        [ValidateNever]
        [ForeignKey("DailyReconciliationId")]
        public DailyReconciliation? DailyReconciliation { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentMethodCode { get; set; } = "CASH";

        [MaxLength(100)]
        public string? PaymentMethodName { get; set; }

        [Required]
        public int TransactionCount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
