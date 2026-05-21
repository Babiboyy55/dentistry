using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhakhoa.Data;
using Nhakhoa.Models;
using Nhakhoa.ViewModels;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Nhakhoa.Controllers
{
    [Authorize]
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Staff
        public async Task<IActionResult> Index()
        {
            var staff = await _context.Users
                .OrderBy(u => u.FullName)
                .Select(u => new StaffListItemViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Username = u.Username,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Role = u.Role,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return View(staff);
        }

        // GET: Staff/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // GET: Staff/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users
                .Include(u => u.StaffProfile)
                .Include(u => u.StaffSalaryInfo)
                .Include(u => u.StaffQualifications)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditStaffViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsActive = user.IsActive,
                StaffCode = user.StaffProfile?.StaffCode,
                PositionTitle = user.StaffProfile?.PositionTitle,
                Department = user.StaffProfile?.Department,
                Gender = user.StaffProfile?.Gender,
                Address = user.StaffProfile?.Address,
                JoinDate = user.StaffProfile?.JoinDate,
                PrimaryClinic = user.StaffProfile?.PrimaryClinic,
                BaseSalary = user.StaffSalaryInfo?.BaseSalary ?? 0m,
                DegreeMultiplier = user.StaffSalaryInfo?.DegreeMultiplier ?? 1m,
                DegreeTitle = user.StaffSalaryInfo?.DegreeTitle,
                RankMultiplier = user.StaffSalaryInfo?.RankMultiplier ?? 1m,
                RankTitle = user.StaffSalaryInfo?.RankTitle,
                SpecializationAllowance = user.StaffSalaryInfo?.SpecializationAllowance ?? 0m,
                SeniorityAllowance = user.StaffSalaryInfo?.SeniorityAllowance ?? 0m,
                MonthlyBonus = user.StaffSalaryInfo?.MonthlyBonus ?? 0m,
                Qualifications = user.StaffQualifications
                    .Select(q => new EditQualificationViewModel
                    {
                        Id = q.Id,
                        Title = q.Title,
                        Major = q.Major,
                        Institution = q.Institution,
                        Year = q.Year,
                        Category = q.Category,
                        ImagePath = q.ImagePath,
                        IsDeleted = false,
                        AcademicRank = q.AcademicRank,
                        AcademicDegree = q.AcademicDegree
                    })
                    .ToList()
            };

            return View(model);
        }

        // GET: Staff/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users
                .Include(u => u.StaffProfile)
                .Include(u => u.StaffSalaryInfo)
                .Include(u => u.StaffQualifications)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var profile = user.StaffProfile;
            var salary = user.StaffSalaryInfo;
            var degreeMultiplier = salary?.DegreeMultiplier ?? 1m;
            var rankMultiplier = salary?.RankMultiplier ?? 1m;

            var model = new StaffProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Role = user.Role,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                StaffCode = profile?.StaffCode,
                PositionTitle = profile?.PositionTitle,
                Department = profile?.Department,
                Gender = profile?.Gender,
                Address = profile?.Address,
                JoinDate = profile?.JoinDate,
                PrimaryClinic = profile?.PrimaryClinic,
                Salary = new StaffSalaryInfoViewModel
                {
                    BaseSalary = salary?.BaseSalary ?? 0m,
                    DegreeMultiplier = degreeMultiplier,
                    DegreeTitle = salary?.DegreeTitle,
                    RankMultiplier = rankMultiplier,
                    RankTitle = salary?.RankTitle,
                    SpecializationAllowance = salary?.SpecializationAllowance ?? 0m,
                    SeniorityAllowance = salary?.SeniorityAllowance ?? 0m,
                    MonthlyBonus = salary?.MonthlyBonus ?? 0m
                },
                Qualifications = user.StaffQualifications
                    .OrderByDescending(q => q.Year)
                    .Select(q => new StaffQualificationViewModel
                    {
                        Title = q.Title,
                        Major = q.Major,
                        Institution = q.Institution,
                        Year = q.Year,
                        Category = q.Category,
                        ImagePath = q.ImagePath,
                        AcademicRank = q.AcademicRank,
                        AcademicDegree = q.AcademicDegree
                    })
                    .ToList()
            };

            return View(model);
        }

        // GET: Staff/Salary/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Salary(int id)
        {
            var user = await _context.Users
                .Include(u => u.StaffProfile)
                .Include(u => u.StaffSalaryInfo)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            var profile = user.StaffProfile;
            var salary = user.StaffSalaryInfo;

            var model = new StaffProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Role = user.Role,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                StaffCode = profile?.StaffCode,
                PositionTitle = profile?.PositionTitle,
                Department = profile?.Department,
                JoinDate = profile?.JoinDate,
                Salary = new StaffSalaryInfoViewModel
                {
                    BaseSalary = salary?.BaseSalary ?? 0m,
                    DegreeMultiplier = salary?.DegreeMultiplier ?? 1m,
                    DegreeTitle = salary?.DegreeTitle,
                    RankMultiplier = salary?.RankMultiplier ?? 1m,
                    RankTitle = salary?.RankTitle,
                    SpecializationAllowance = salary?.SpecializationAllowance ?? 0m,
                    SeniorityAllowance = salary?.SeniorityAllowance ?? 0m,
                    MonthlyBonus = salary?.MonthlyBonus ?? 0m
                },
                Qualifications = new List<StaffQualificationViewModel>()
            };

            return View(model);
        }

        // POST: Staff/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateStaffViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra user đã tồn tại chưa
                if (_context.Users.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }

                var user = new User
                {
                    FullName = model.FullName,
                    Username = model.Username,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Role = model.Role,
                    PasswordHash = model.Password,
                    IsActive = true,
                    IsTemporaryPassword = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Lưu lịch sử hoạt động
                var currentUser = User.Identity?.Name ?? "System";
                var log = new ActivityLog
                {
                    Username = currentUser,
                    Action = "Tạo tài khoản",
                    Details = $"Tạo mới tài khoản nhân sự: {user.FullName} ({user.Username}) với vai trò {user.Role}"
                };
                _context.ActivityLogs.Add(log);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tài khoản đã được tạo thành công";
                return RedirectToAction(nameof(Details), new { id = user.Id });
            }

            return View(model);
        }

        // POST: Staff/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(EditStaffViewModel model)
        {
            // Manual validation for Qualifications
            if (model.Qualifications != null)
            {
                for (int i = 0; i < model.Qualifications.Count; i++)
                {
                    var q = model.Qualifications[i];
                    if (!q.IsDeleted && string.IsNullOrWhiteSpace(q.Title))
                    {
                        ModelState.AddModelError($"Qualifications[{i}].Title", "Vui lòng nhập tên bằng cấp/chứng chỉ");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .Include(u => u.StaffProfile)
                .Include(u => u.StaffSalaryInfo)
                .Include(u => u.StaffQualifications)
                .FirstOrDefaultAsync(u => u.Id == model.Id);
            if (user == null)
            {
                return NotFound();
            }

            var oldRole = user.Role;
            var oldStatus = user.IsActive;

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Role = model.Role;
            user.IsActive = model.IsActive;

            if (user.StaffProfile == null)
            {
                user.StaffProfile = new StaffProfile();
                _context.StaffProfiles.Add(user.StaffProfile);
            }
            user.StaffProfile.UserId = user.Id;
            user.StaffProfile.User = user;
            user.StaffProfile.StaffCode = model.StaffCode;
            user.StaffProfile.PositionTitle = model.PositionTitle;
            user.StaffProfile.Department = model.Department;
            user.StaffProfile.Gender = model.Gender;
            user.StaffProfile.Address = model.Address;
            user.StaffProfile.JoinDate = model.JoinDate;
            user.StaffProfile.PrimaryClinic = model.PrimaryClinic;

            if (user.StaffSalaryInfo == null)
            {
                user.StaffSalaryInfo = new StaffSalaryInfo();
                _context.StaffSalaryInfos.Add(user.StaffSalaryInfo);
            }
            user.StaffSalaryInfo.UserId = user.Id;
            user.StaffSalaryInfo.User = user;
            user.StaffSalaryInfo.BaseSalary = model.BaseSalary ?? 0m;
            user.StaffSalaryInfo.DegreeMultiplier = model.DegreeMultiplier ?? 1m;
            user.StaffSalaryInfo.DegreeTitle = model.DegreeTitle;
            user.StaffSalaryInfo.RankMultiplier = model.RankMultiplier ?? 1m;
            user.StaffSalaryInfo.RankTitle = model.RankTitle;
            user.StaffSalaryInfo.SpecializationAllowance = model.SpecializationAllowance ?? 0m;
            user.StaffSalaryInfo.SeniorityAllowance = model.SeniorityAllowance ?? 0m;
            user.StaffSalaryInfo.MonthlyBonus = model.MonthlyBonus ?? 0m;

            // --- Process Qualifications ---
            if (model.Qualifications != null)
            {
                foreach (var qModel in model.Qualifications)
                {
                    if (qModel.IsDeleted)
                    {
                        if (qModel.Id > 0)
                        {
                            var existing = user.StaffQualifications.FirstOrDefault(q => q.Id == qModel.Id);
                            if (existing != null)
                            {
                                _context.StaffQualifications.Remove(existing);
                                DeleteQualificationImage(existing.ImagePath);
                            }
                        }
                    }
                    else if (qModel.Id == 0)
                    {
                        if (!string.IsNullOrWhiteSpace(qModel.Title))
                        {
                            var newQual = new StaffQualification
                            {
                                UserId = user.Id,
                                User = user,
                                Title = qModel.Title,
                                Major = qModel.Major ?? "",
                                Institution = qModel.Institution ?? "",
                                Year = qModel.Year,
                                Category = qModel.Category ?? "Degree",
                                AcademicRank = qModel.AcademicRank,
                                AcademicDegree = qModel.AcademicDegree
                            };

                            if (qModel.ImageFile != null)
                            {
                                newQual.ImagePath = await SaveQualificationImageAsync(qModel.ImageFile);
                            }

                            _context.StaffQualifications.Add(newQual);
                        }
                    }
                    else
                    {
                        var existing = user.StaffQualifications.FirstOrDefault(q => q.Id == qModel.Id);
                        if (existing != null)
                        {
                            existing.Title = qModel.Title;
                            existing.Major = qModel.Major ?? "";
                            existing.Institution = qModel.Institution ?? "";
                            existing.Year = qModel.Year;
                            existing.Category = qModel.Category ?? "Degree";
                            existing.AcademicRank = qModel.AcademicRank;
                            existing.AcademicDegree = qModel.AcademicDegree;

                            if (qModel.ImageFile != null)
                            {
                                DeleteQualificationImage(existing.ImagePath);
                                existing.ImagePath = await SaveQualificationImageAsync(qModel.ImageFile);
                            }
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            // Lưu lịch sử hoạt động
            var currentUser = User.Identity?.Name ?? "System";
            var details = $"Cập nhật thông tin nhân sự và hồ sơ: {user.FullName} ({user.Username}).";
            if (oldRole != model.Role) details += $" Đổi vai trò: {oldRole} -> {model.Role}.";
            if (oldStatus != model.IsActive) details += $" Trạng thái: {(oldStatus ? "Hoạt động" : "Bị khóa")} -> {(model.IsActive ? "Hoạt động" : "Bị khóa")}.";

            var log = new ActivityLog
            {
                Username = currentUser,
                Action = "Cập nhật thông tin",
                Details = details
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công";
            return RedirectToAction(nameof(Details), new { id = user.Id });
        }

        // GET: Staff/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = User.Identity?.Name;
            if (user.Username.Equals(currentUser, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Bạn không thể tự xóa tài khoản của chính mình.";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // POST: Staff/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = User.Identity?.Name;
            if (user.Username.Equals(currentUser, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Bạn không thể tự xóa tài khoản của chính mình.";
                return RedirectToAction(nameof(Index));
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            var log = new ActivityLog
            {
                Username = currentUser ?? "System",
                Action = "Xóa tài khoản",
                Details = $"Đã xóa tài khoản nhân sự: {user.FullName} ({user.Username})"
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa tài khoản {user.FullName} thành công.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Staff/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            // Không cho phép tự khóa tài khoản của chính mình khi đang đăng nhập
            var currentUser = User.Identity?.Name;
            if (user.Username.Equals(currentUser, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Bạn không thể tự khóa tài khoản của chính mình.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            // Lưu lịch sử hoạt động
            var actionType = user.IsActive ? "Mở khóa tài khoản" : "Khóa tài khoản";
            var log = new ActivityLog
            {
                Username = currentUser ?? "System",
                Action = actionType,
                Details = $"{actionType} của nhân sự: {user.FullName} ({user.Username})"
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{(user.IsActive ? "Mở khóa" : "Khóa")} tài khoản nhân sự {user.FullName} thành công.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Staff/ResetPassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var defaultPassword = "123456";
            user.PasswordHash = defaultPassword;
            user.IsTemporaryPassword = true;
            user.SecurityStamp = Guid.NewGuid().ToString();
            await _context.SaveChangesAsync();

            // Lưu lịch sử hoạt động
            var currentUser = User.Identity?.Name ?? "System";
            var log = new ActivityLog
            {
                Username = currentUser,
                Action = "Cấp lại mật khẩu",
                Details = $"Cấp lại mật khẩu mặc định (123456) cho nhân sự: {user.FullName} ({user.Username})"
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Cấp lại mật khẩu mặc định (123456) cho {user.FullName} thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Staff/ActivityLog
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivityLog()
        {
            var logs = await _context.ActivityLogs
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            return View(logs);
        }

        private async Task<string?> SaveQualificationImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "qualifications");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/qualifications/" + uniqueFileName;
        }

        private void DeleteQualificationImage(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch
                {
                    // Ignore
                }
            }
        }
    }
}
