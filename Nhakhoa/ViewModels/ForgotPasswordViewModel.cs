using System.ComponentModel.DataAnnotations;

namespace Nhakhoa.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập hoặc email")]
        public string UsernameOrEmail { get; set; }
    }
}
