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
        public decimal? SpecializationAllowance { get; set; }
        public decimal? SeniorityAllowance { get; set; }
        public decimal? MonthlyBonus { get; set; }
    }
}
