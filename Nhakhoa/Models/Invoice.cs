using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Nhakhoa.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string InvoiceCode { get; set; } = string.Empty;

        public int? PatientId { get; set; }
        [ValidateNever]
        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // CASH, BANK, VNPAY, MOMO
        [Required]
        [MaxLength(50)]
        public string PaymentMethodCode { get; set; } = "CASH";

        // "Chờ thanh toán", "Đã thanh toán", "Đã hủy", "Đã hoàn tiền"
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Chờ thanh toán";

        [Required]
        public DateTime IssuedAt { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        // New properties from database snapshot
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal VATPercent { get; set; } = 10m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal VATAmount { get; set; }

        public DateTime? PaidAt { get; set; }

        [MaxLength(100)]
        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public int? ExaminationSessionId { get; set; }
        [ValidateNever]
        [ForeignKey("ExaminationSessionId")]
        public ExaminationSession? ExaminationSession { get; set; }

        // Navigation properties
        [ValidateNever]
        public ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();

        [ValidateNever]
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        [ValidateNever]
        public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
    }
}
