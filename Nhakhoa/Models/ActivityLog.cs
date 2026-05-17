using System;
using System.ComponentModel.DataAnnotations;

namespace Nhakhoa.Models
{
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }

        public string Username { get; set; } // Username of the staff performing the action
        
        [Required]
        public string Action { get; set; } // Type of action: "Đăng nhập", "Tạo tài khoản", "Khóa tài khoản", "Cấp lại mật khẩu", v.v.

        [Required]
        public string Details { get; set; } // Details: "Khóa tài khoản nhân viên Nguyễn Văn A"

        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
