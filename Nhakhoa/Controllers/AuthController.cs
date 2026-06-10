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
                if (model.Role == "Patient")
                {
                    var account = await _context.PatientAccounts.FirstOrDefaultAsync(pa => pa.PhoneNumber == model.Username);
                    if (account != null)
                    {
                        if (account.LockoutEnd != null && account.LockoutEnd > DateTime.Now)
                        {
                            var remaining = account.LockoutEnd.Value - DateTime.Now;
                            ModelState.AddModelError(string.Empty, $"Tài khoản bị tạm khóa do nhập sai nhiều lần. Vui lòng thử lại sau {Math.Ceiling(remaining.TotalMinutes)} phút.");
                            return View(model);
                        }

                        if (!account.IsActive)
                        {
                            ModelState.AddModelError(string.Empty, "Tài khoản bệnh nhân chưa được kích hoạt hoặc đã bị khóa.");
                            return View(model);
                        }

                        if (account.PasswordHash == model.Password)
                        {
                            account.FailedLoginAttempts = 0;
                            account.LockoutEnd = null;

                            var random = new Random();
                            var otp = random.Next(100000, 999999).ToString();
                            account.OtpCode = otp;
                            account.OtpExpiry = DateTime.Now.AddMinutes(5);

                            _context.PatientAccounts.Update(account);
                            await _context.SaveChangesAsync();

                            var patientLog = new ActivityLog
                            {
                                Username = account.PhoneNumber,
                                Action = "Yêu cầu 2FA",
                                Details = $"Bệnh nhân {account.FullName} đăng nhập. Mã OTP 2FA (giả lập): {otp}"
                            };
                            _context.ActivityLogs.Add(patientLog);
                            await _context.SaveChangesAsync();

                            TempData["MockedOtp"] = otp;
                            return RedirectToAction("VerifyPatientOtp", new { phone = account.PhoneNumber, flow = "Login2FA" });
                        }
                        else
                        {
                            account.FailedLoginAttempts++;
                            if (account.FailedLoginAttempts >= 5)
                            {
                                account.LockoutEnd = DateTime.Now.AddMinutes(15);
                                var patientLockLog = new ActivityLog
                                {
                                    Username = account.PhoneNumber,
                                    Action = "Tài khoản bị khóa",
                                    Details = $"Tài khoản bệnh nhân {account.PhoneNumber} bị khóa tạm thời 15 phút do sai mật khẩu 5 lần."
                                };
                                _context.ActivityLogs.Add(patientLockLog);
                            }
                            _context.PatientAccounts.Update(account);
                            await _context.SaveChangesAsync();
                        }
                    }

                    ModelState.AddModelError(string.Empty, "Số điện thoại hoặc mật khẩu không đúng.");
                    return View(model);
                }

                // In production, compare with hashed password
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username && u.PasswordHash == model.Password);

                if (user != null)
                {
                    if (!user.IsActive)
                    {
                        // Ghi nhận đăng nhập thất bại do tài khoản bị khóa
                        var failLog = new ActivityLog
                        {
                            Username = model.Username ?? "Unknown",
                            Action = "Đăng nhập thất bại",
                            Details = $"Đăng nhập thất bại: Tài khoản '{model.Username}' đang bị khóa."
                        };
                        _context.ActivityLogs.Add(failLog);
                        await _context.SaveChangesAsync();

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

                // Ghi nhận đăng nhập thất bại do sai mật khẩu hoặc tên đăng nhập
                var failedCredsLog = new ActivityLog
                {
                    Username = model.Username ?? "Unknown",
                    Action = "Đăng nhập thất bại",
                    Details = $"Đăng nhập thất bại cho tài khoản '{model.Username}'."
                };
                _context.ActivityLogs.Add(failedCredsLog);
                await _context.SaveChangesAsync();

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
                if (model.Role == "Patient")
                {
                    var existingActive = await _context.PatientAccounts.AnyAsync(pa => pa.PhoneNumber == model.PhoneNumber && pa.IsActive);
                    if (existingActive)
                    {
                        ModelState.AddModelError("PhoneNumber", "Số điện thoại này đã được đăng ký tài khoản. Vui lòng đăng nhập hoặc chọn Quên mật khẩu.");
                        return View(model);
                    }

                    var random = new Random();
                    var otp = random.Next(100000, 999999).ToString();

                    var account = await _context.PatientAccounts.FirstOrDefaultAsync(pa => pa.PhoneNumber == model.PhoneNumber);
                    if (account == null)
                    {
                        account = new PatientAccount
                        {
                            PhoneNumber = model.PhoneNumber,
                            FullName = model.FullName,
                            Email = model.Email,
                            PasswordHash = model.Password,
                            IsActive = false,
                            OtpCode = otp,
                            OtpExpiry = DateTime.Now.AddMinutes(5),
                            SecurityStamp = Guid.NewGuid().ToString()
                        };
                        _context.PatientAccounts.Add(account);
                    }
                    else
                    {
                        account.FullName = model.FullName;
                        account.Email = model.Email;
                        account.PasswordHash = model.Password;
                        account.OtpCode = otp;
                        account.OtpExpiry = DateTime.Now.AddMinutes(5);
                        _context.PatientAccounts.Update(account);
                    }

                    await _context.SaveChangesAsync();

                    var patientRegLog = new ActivityLog
                    {
                        Username = model.PhoneNumber,
                        Action = "Đăng ký OTP",
                        Details = $"Yêu cầu đăng ký tài khoản bệnh nhân {model.FullName}. Mã OTP đăng ký (giả lập): {otp}"
                    };
                    _context.ActivityLogs.Add(patientRegLog);
                    await _context.SaveChangesAsync();

                    TempData["MockedOtp"] = otp;
                    return RedirectToAction("VerifyPatientOtp", new { phone = model.PhoneNumber, flow = "Register" });
                }

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

        // === PATIENT PORTAL AUTHENTICATION ===

        [HttpGet]
        public IActionResult VerifyPatientOtp(string phone, string flow)
        {
            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(flow))
            {
                return RedirectToAction("Login");
            }

            var model = new VerifyPatientOtpViewModel { PhoneNumber = phone, Flow = flow };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyPatientOtp(VerifyPatientOtpViewModel model)
        {
            if (ModelState.IsValid)
            {
                var account = await _context.PatientAccounts.FirstOrDefaultAsync(pa => pa.PhoneNumber == model.PhoneNumber);
                if (account == null)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản không hợp lệ.");
                    return View(model);
                }

                if (account.OtpCode != model.OtpCode)
                {
                    ModelState.AddModelError("OtpCode", "Mã OTP không đúng.");
                    return View(model);
                }

                if (account.OtpExpiry == null || account.OtpExpiry < DateTime.Now)
                {
                    ModelState.AddModelError("OtpCode", "Mã OTP đã hết hạn.");
                    return View(model);
                }

                // OTP is valid!
                if (model.Flow == "Register")
                {
                    account.IsActive = true;
                    account.OtpCode = null;
                    account.OtpExpiry = null;
                    
                    // Link with internal Patient record if matching phone exists
                    var internalPatient = await _context.Patients.FirstOrDefaultAsync(p => p.PhoneNumber == account.PhoneNumber);
                    if (internalPatient != null)
                    {
                        account.PatientId = internalPatient.Id;
                    }

                    _context.PatientAccounts.Update(account);
                    await _context.SaveChangesAsync();

                    var log = new ActivityLog
                    {
                        Username = account.PhoneNumber,
                        Action = "Xác nhận OTP đăng ký",
                        Details = $"Kích hoạt tài khoản bệnh nhân thành công cho SĐT {account.PhoneNumber}."
                    };
                    _context.ActivityLogs.Add(log);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Đăng ký tài khoản bệnh nhân thành công! Vui lòng đăng nhập.";
                    return RedirectToAction("Login");
                }
                else if (model.Flow == "Login2FA")
                {
                    account.OtpCode = null;
                    account.OtpExpiry = null;
                    _context.PatientAccounts.Update(account);
                    await _context.SaveChangesAsync();

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, account.PhoneNumber),
                        new Claim(ClaimTypes.Role, "Patient"),
                        new Claim("UserId", account.Id.ToString()),
                        new Claim("SecurityStamp", account.SecurityStamp)
                    };

                    var identity = new ClaimsIdentity(claims, "Cookies");
                    var principal = new ClaimsPrincipal(identity);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTime.UtcNow.AddHours(8)
                    };

                    await HttpContext.SignInAsync("Cookies", principal, authProperties);

                    var log = new ActivityLog
                    {
                        Username = account.PhoneNumber,
                        Action = "Đăng nhập Patient Portal",
                        Details = $"Bệnh nhân {account.FullName} đã xác thực 2FA và đăng nhập vào portal."
                    };
                    _context.ActivityLogs.Add(log);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "PatientPortal");
                }
                else if (model.Flow == "ForgotPassword")
                {
                    // Redirect to ResetPassword page for Patient
                    return RedirectToAction("ResetPatientPassword", new { phone = account.PhoneNumber, otp = model.OtpCode });
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPatientPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPatientPassword(PatientForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var account = await _context.PatientAccounts.FirstOrDefaultAsync(pa => pa.PhoneNumber == model.PhoneNumber && pa.IsActive);
                if (account != null)
                {
                    var random = new Random();
                    var otp = random.Next(100000, 999999).ToString();

                    account.OtpCode = otp;
                    account.OtpExpiry = DateTime.UtcNow.AddMinutes(5);

                    _context.PatientAccounts.Update(account);
                    await _context.SaveChangesAsync();

                    TempData["MockedOtp"] = otp;

                    var log = new ActivityLog
                    {
                        Username = account.PhoneNumber,
                        Action = "Yêu cầu khôi phục mật khẩu",
                        Details = $"Bệnh nhân {account.FullName} yêu cầu khôi phục mật khẩu. Mã OTP (giả lập): {otp}"
                    };
                    _context.ActivityLogs.Add(log);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("VerifyPatientOtp", new { phone = account.PhoneNumber, flow = "ForgotPassword" });
                }

                // Generic message for security
                TempData["Message"] = "Nếu số điện thoại tồn tại trên hệ thống, bạn sẽ nhận được mã OTP.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPatientPassword(string phone, string otp)
        {
            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(otp))
            {
                return RedirectToAction("Login");
            }

            var model = new PatientResetPasswordViewModel { PhoneNumber = phone, OtpCode = otp };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPatientPassword(PatientResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var account = await _context.PatientAccounts.FirstOrDefaultAsync(pa => pa.PhoneNumber == model.PhoneNumber && pa.IsActive);
                if (account == null)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản không hợp lệ.");
                    return View(model);
                }

                if (account.OtpCode != model.OtpCode || account.OtpExpiry == null || account.OtpExpiry < DateTime.UtcNow)
                {
                    ModelState.AddModelError(string.Empty, "Mã xác thực không hợp lệ hoặc đã hết hạn.");
                    return View(model);
                }

                account.PasswordHash = model.NewPassword;
                account.OtpCode = null;
                account.OtpExpiry = null;
                account.SecurityStamp = Guid.NewGuid().ToString(); // Invalidate all existing sessions

                _context.PatientAccounts.Update(account);
                await _context.SaveChangesAsync();

                var log = new ActivityLog
                {
                    Username = account.PhoneNumber,
                    Action = "Khôi phục mật khẩu thành công",
                    Details = $"Bệnh nhân {account.FullName} đã khôi phục mật khẩu thành công."
                };
                _context.ActivityLogs.Add(log);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Mật khẩu đã được khôi phục thành công. Vui lòng đăng nhập lại.";
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
