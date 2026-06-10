using System.ComponentModel.DataAnnotations;

namespace Nhakhoa.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public string Role { get; set; } = "Staff"; // "Staff" or "Patient"
    }
}
