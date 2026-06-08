using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Nhakhoa.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InvoiceId { get; set; }
        [ValidateNever]
        [ForeignKey("InvoiceId")]
        public Invoice? Invoice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentMethodCode { get; set; } = "CASH";

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Chờ thanh toán";

        [MaxLength(100)]
        public string? TransactionCode { get; set; }

        [MaxLength(500)]
        public string? TransactionReference { get; set; }

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? PaidAt { get; set; }

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
