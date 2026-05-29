using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nhakhoa.Models
{
    public class StaffSalaryInfo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public User User { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal DegreeMultiplier { get; set; } = 1m;

        [MaxLength(100)]
        public string? DegreeTitle { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal RankMultiplier { get; set; } = 1m;

        [MaxLength(100)]
        public string? RankTitle { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SpecializationAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SeniorityAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyBonus { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherDeductions { get; set; }

        [MaxLength(100)]
        public string? PendingRankTitle { get; set; }

        public bool IsRankChangePending { get; set; } = false;
    }
}
