using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Nhakhoa.Models
{
    public class RefundApproval
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RefundId { get; set; }
        [ValidateNever]
        [ForeignKey("RefundId")]
        public Refund? Refund { get; set; }

        [Required]
        public int ApprovalLevel { get; set; } = 1;

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Chờ duyệt";

        [MaxLength(500)]
        public string? Comment { get; set; }

        [MaxLength(100)]
        public string? ApprovedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ApprovedAt { get; set; }
    }
}
