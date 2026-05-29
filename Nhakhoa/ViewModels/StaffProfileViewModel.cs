using System;
using System.Collections.Generic;

namespace Nhakhoa.ViewModels
{
    public class StaffProfileViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }

        public string StaffCode { get; set; }
        public string PositionTitle { get; set; }
        public string Department { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public DateTime? JoinDate { get; set; }
        public string PrimaryClinic { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Cccd { get; set; }
        public string? CchnNumber { get; set; }
        public DateTime? CchnIssueDate { get; set; }
        public DateTime? CchnExpiryDate { get; set; }
        public string? CchnProvider { get; set; }
        public string? AcademicRank { get; set; }
        public string? AcademicDegree { get; set; }
        public string? JobRank { get; set; }
        public int? ExperienceYears { get; set; }

        public StaffSalaryInfoViewModel Salary { get; set; } = new StaffSalaryInfoViewModel();
        public List<StaffQualificationViewModel> Qualifications { get; set; } = new List<StaffQualificationViewModel>();
    }

    public class StaffSalaryInfoViewModel
    {
        public decimal BaseSalary { get; set; }
        public decimal DegreeMultiplier { get; set; } = 1m;
        public string? DegreeTitle { get; set; }
        public decimal RankMultiplier { get; set; } = 1m;
        public string? RankTitle { get; set; }
        public decimal SpecializationAllowance { get; set; }
        public decimal SeniorityAllowance { get; set; }
        public decimal MonthlyBonus { get; set; }
        public decimal OtherDeductions { get; set; }
        public string? PendingRankTitle { get; set; }
        public bool IsRankChangePending { get; set; }

        public decimal MonthlyTotal => (BaseSalary * DegreeMultiplier * RankMultiplier) + SpecializationAllowance + SeniorityAllowance + MonthlyBonus;
    }

    public class StaffQualificationViewModel
    {
        public string Title { get; set; }
        public string Major { get; set; }
        public string Institution { get; set; }
        public int? Year { get; set; }
        public string Category { get; set; }
        public string? ImagePath { get; set; }
        public string? AcademicRank { get; set; }
        public string? AcademicDegree { get; set; }
    }
}
