using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Nhakhoa.Models
{
    public class DailyReconciliation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime ReconciliationDate { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Khớp";

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalInvoiceAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCollectedAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DifferenceAmount { get; set; }

        [MaxLength(1000)]
        public string? DifferenceNotes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [MaxLength(100)]
        public string? ApprovedBy { get; set; }

        [ValidateNever]
        public ICollection<ReconciliationDetail> Details { get; set; } = new List<ReconciliationDetail>();
    }
}
