using System.ComponentModel.DataAnnotations;

namespace Nhakhoa.ViewModels
{
    public class VerifyOtpViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có đúng 6 chữ số")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Mã OTP chỉ bao gồm chữ số")]
        public string OtpCode { get; set; }
    }
}
