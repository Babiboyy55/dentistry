using System;
using System.ComponentModel.DataAnnotations;

namespace Nhakhoa.Models
{
    public class PaymentMethod
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty; // CASH, BANK, VNPAY, MOMO, INSURANCE

        [Required]
        public bool IsEnabled { get; set; } = true;

        public bool IsDigitalGateway { get; set; } = false;

        [MaxLength(100)]
        public string? MerchantId { get; set; }

        [MaxLength(500)]
        public string? SecretKey { get; set; } // AES-256 encrypted

        [MaxLength(50)]
        public string? Environment { get; set; } // Sandbox or Production

        [MaxLength(500)]
        public string? EndpointUrl { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
