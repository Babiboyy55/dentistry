using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Nhakhoa.Models
{
    public class Refund
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
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RefundMethodCode { get; set; } = "CASH";

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Chờ duyệt";

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [MaxLength(100)]
        public string? ApprovedBy { get; set; }

        public DateTime? RefundedAt { get; set; }

        [MaxLength(100)]
        public string? RefundBy { get; set; }

        [ValidateNever]
        public ICollection<RefundApproval> ApprovalHistory { get; set; } = new List<RefundApproval>();
    }
}
