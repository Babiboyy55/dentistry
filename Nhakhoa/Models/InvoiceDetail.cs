using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Nhakhoa.Models
{
    public class InvoiceDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InvoiceId { get; set; }
        [ValidateNever]
        [ForeignKey("InvoiceId")]
        public Invoice? Invoice { get; set; }

        [Required]
        public int MedicalServiceId { get; set; }
        [ValidateNever]
        [ForeignKey("MedicalServiceId")]
        public MedicalService? MedicalService { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
