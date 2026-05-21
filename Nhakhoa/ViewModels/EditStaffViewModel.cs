using System;
using System.ComponentModel.DataAnnotations;

namespace Nhakhoa.ViewModels
{
    public class EditStaffViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; }

        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        public string Role { get; set; }

        public bool IsActive { get; set; }

        // --- Profile Info ---
        public string? StaffCode { get; set; }
        public string? PositionTitle { get; set; }
        public string? Department { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public DateTime? JoinDate { get; set; }
        public string? PrimaryClinic { get; set; }

        // --- Salary Info ---
        public decimal? BaseSalary { get; set; }
        public decimal? DegreeMultiplier { get; set; } = 1m;
        public string? DegreeTitle { get; set; }
        public decimal? RankMultiplier { get; set; } = 1m;
        public string? RankTitle { get; set; }
        public decimal? SpecializationAllowance { get; set; }
        public decimal? SeniorityAllowance { get; set; }
        public decimal? MonthlyBonus { get; set; }

        // --- Qualifications ---
        public List<EditQualificationViewModel> Qualifications { get; set; } = new List<EditQualificationViewModel>();
    }

    public class EditQualificationViewModel
    {
        public int Id { get; set; }
        
        public string Title { get; set; }
        
        public string? Major { get; set; }
        
        public string? Institution { get; set; }
        
        public int? Year { get; set; }
        
        [Required]
        public string Category { get; set; } = "Degree"; // Degree, Certificate
        
        public string? ImagePath { get; set; }
        
        public Microsoft.AspNetCore.Http.IFormFile? ImageFile { get; set; }
        
        public bool IsDeleted { get; set; } = false;

        public string? AcademicRank { get; set; }

        public string? AcademicDegree { get; set; }
    }
}
