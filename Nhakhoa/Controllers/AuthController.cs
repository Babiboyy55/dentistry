using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhakhoa.Data;
using Nhakhoa.Models;
using Nhakhoa.ViewModels;
using System.Security.Claims;

namespace Nhakhoa.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // In production, compare with hashed password
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username && u.PasswordHash == model.Password);

                if (user != null)
                {
                    if (!user.IsActive)
                    {
                        ModelState.AddModelError(string.Empty, "Tài khoản bị khóa.");
                        return View(model);
                    }

                    // Đảm bảo SecurityStamp luôn có giá trị
                    if (string.IsNullOrEmpty(user.SecurityStamp))
                    {
                        user.SecurityStamp = Guid.NewGuid().ToString();
                        await _context.SaveChangesAsync();
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim("UserId", user.Id.ToString()),
                        new Claim("SecurityStamp", user.SecurityStamp)
                    };

                    var identity = new ClaimsIdentity(claims, "Cookies");
                    var principal = new ClaimsPrincipal(identity);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(8) // Session timeout automatic
                    };

                    await HttpContext.SignInAsync("Cookies", principal, authProperties);

                    // Ghi lịch sử hoạt động đăng nhập
                    var log = new ActivityLog
                    {
                        Username = user.Username,
                        Action = "Đăng nhập",
                        Details = $"Nhân sự {user.FullName} ({user.Username}) đã đăng nhập vào hệ thống."
                    };
                    _context.ActivityLogs.Add(log);
                    await _context.SaveChangesAsync();

                    return Redirect("/Home/Index?clear=1");
                }

                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return Redirect("/Home/Index?clear=1");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var isExist = await _context.Users.AnyAsync(u => u.Username == model.Username);
                if (isExist)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }

                var user = new User
                {
                    Username = model.Username,
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    PasswordHash = model.Password,
                    Role = model.Role,
                    IsActive = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var log = new ActivityLog
                {
                    Username = user.Username,
                    Action = "Đăng ký tài khoản",
                    Details = $"Người dùng {user.FullName} ({user.Username}) đã tự đăng ký tài khoản thành công với vai trò {user.Role}."
                };
                _context.ActivityLogs.Add(log);
                await _context.SaveChangesAsync();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("SecurityStamp", user.SecurityStamp)
                };

                var identity = new ClaimsIdentity(claims, "Cookies");
                var principal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTime.UtcNow.AddHours(8)
                };

                await HttpContext.SignInAsync("Cookies", principal, authProperties);

                return Redirect("/Home/Index?clear=1");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Debug()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            var dbStamp = "";
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (int.TryParse(userIdClaim, out int uid))
            {
                var user = _context.Users.Find(uid);
                dbStamp = user?.SecurityStamp;
            }
            return Json(new { isAuthenticated = User.Identity.IsAuthenticated, claims, dbStamp });
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user != null)
                    {
                        if (user.PasswordHash != model.CurrentPassword)
                        {
                            ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                            return View(model);
                        }

                        if (model.NewPassword == model.CurrentPassword)
                        {
                            ModelState.AddModelError("NewPassword", "Mật khẩu mới không được trùng mật khẩu cũ.");
                            return View(model);
                        }

                        user.PasswordHash = model.NewPassword;
                        user.IsTemporaryPassword = false;
                        user.SecurityStamp = Guid.NewGuid().ToString();

                        _context.Users.Update(user);
                        await _context.SaveChangesAsync();

                        var log = new ActivityLog
                        {
                            Username = user.Username,
                            Action = "Đổi mật khẩu",
                            Details = $"Người dùng {user.FullName} ({user.Username}) đã đổi mật khẩu thành công."
                        };
                        _context.ActivityLogs.Add(log);
                        await _context.SaveChangesAsync();

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.Username),
                            new Claim(ClaimTypes.Role, user.Role),
                            new Claim("UserId", user.Id.ToString()),
                            new Claim("SecurityStamp", user.SecurityStamp)
                        };

                        var identity = new ClaimsIdentity(claims, "Cookies");
                        var principal = new ClaimsPrincipal(identity);

                        await HttpContext.SignInAsync("Cookies", principal, new AuthenticationProperties
                        {
                            IsPersistent = false,
                            ExpiresUtc = DateTime.UtcNow.AddHours(8)
                        });

                        TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                        return Redirect("/Home/Index?clear=1");
                    }
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity.IsAuthenticated)
            {
                return Redirect("/Home/Index?clear=1");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.IsActive && 
                    (u.Username == model.UsernameOrEmail || u.Email == model.UsernameOrEmail));

                if (user != null)
                {
                    var random = new Random();
                    var otp = random.Next(100000, 999999).ToString();

                    user.ResetOtpCode = otp;
                    user.ResetOtpExpiry = DateTime.UtcNow.AddMinutes(5);

                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    TempData["MockedOtp"] = otp;
                    TempData["Message"] = $"Một mã OTP đã được gửi tới email/SĐT đã đăng ký.";

                    var log = new ActivityLog
                    {
                        Username = "System",
                        Action = "Yêu cầu khôi phục mật khẩu",
                        Details = $"Đã tạo mã OTP khôi phục mật khẩu cho {user.Username}. Mã OTP (giả lập): {otp}"
                    };
                    _context.ActivityLogs.Add(log);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("VerifyOtp", new { username = user.Username });
                }

                TempData["Message"] = "Nếu thông tin hợp lệ, bạn sẽ nhận được hướng dẫn qua email/SMS.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyOtp(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login");
            }

            var model = new VerifyOtpViewModel { Username = username };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username && u.IsActive);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản không hợp lệ hoặc đã bị vô hiệu hóa.");
                    return View(model);
                }

                if (user.ResetOtpCode != model.OtpCode)
                {
                    ModelState.AddModelError("OtpCode", "Mã OTP không hợp lệ.");
                    return View(model);
                }

                if (user.ResetOtpExpiry == null || user.ResetOtpExpiry < DateTime.UtcNow)
                {
                    ModelState.AddModelError("OtpCode", "Mã OTP đã hết hạn.");
                    return View(model);
                }

                return RedirectToAction("ResetPassword", new { username = user.Username, otp = model.OtpCode });
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string username, string otp)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(otp))
            {
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel { Username = username, OtpCode = otp };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username && u.IsActive);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản không hợp lệ.");
                    return View(model);
                }

                if (user.ResetOtpCode != model.OtpCode || user.ResetOtpExpiry == null || user.ResetOtpExpiry < DateTime.UtcNow)
                {
                    ModelState.AddModelError(string.Empty, "Yêu cầu khôi phục mật khẩu không hợp lệ hoặc đã hết hạn.");
                    return View(model);
                }

                user.PasswordHash = model.NewPassword;
                user.ResetOtpCode = null;
                user.ResetOtpExpiry = null;
                user.SecurityStamp = Guid.NewGuid().ToString();

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                var log = new ActivityLog
                {
                    Username = user.Username,
                    Action = "Khôi phục mật khẩu",
                    Details = $"Tài khoản {user.Username} đã khôi phục mật khẩu thành công qua mã OTP."
                };
                _context.ActivityLogs.Add(log);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Mật khẩu đã được đặt lại thành công. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Login", "Auth");
        }
    }
}
