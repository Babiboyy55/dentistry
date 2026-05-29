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
            var username = User.Identity?.Name;
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (currentUser == null) return Unauthorized();

            // Ràng buộc bảo mật (ALT-1) - Chỉ Admin mới có quyền truy cập trang sửa
            if (currentUser.Role != "Admin")
            {
                var editGetLog = new ActivityLog
                {
                    Username = username ?? "System",
                    Action = "Chỉnh sửa trái phép",
                    Details = $"Người dùng {username} (vai trò {currentUser.Role}) cố gắng chỉnh sửa trái phép hồ sơ của nhân sự ID {id}."
                };
                _context.ActivityLogs.Add(editGetLog);
                await _context.SaveChangesAsync();
                return StatusCode(StatusCodes.Status403Forbidden);
            }

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
                
                // New profile fields
                DateOfBirth = user.StaffProfile?.DateOfBirth,
                Cccd = user.StaffProfile?.Cccd,
                CchnNumber = user.StaffProfile?.CchnNumber,
                CchnIssueDate = user.StaffProfile?.CchnIssueDate,
                CchnExpiryDate = user.StaffProfile?.CchnExpiryDate,
                CchnProvider = user.StaffProfile?.CchnProvider,
                AcademicRank = user.StaffProfile?.AcademicRank,
                AcademicDegree = user.StaffProfile?.AcademicDegree,
                JobRank = user.StaffProfile?.JobRank,
                ExperienceYears = user.StaffProfile?.ExperienceYears,

                // Salary info
                BaseSalary = user.StaffSalaryInfo?.BaseSalary ?? 0m,
                DegreeMultiplier = user.StaffSalaryInfo?.DegreeMultiplier ?? 1m,
                DegreeTitle = user.StaffSalaryInfo?.DegreeTitle,
                RankMultiplier = user.StaffSalaryInfo?.RankMultiplier ?? 1m,
                RankTitle = user.StaffSalaryInfo?.RankTitle,
                SpecializationAllowance = user.StaffSalaryInfo?.SpecializationAllowance ?? 0m,
                SeniorityAllowance = user.StaffSalaryInfo?.SeniorityAllowance ?? 0m,
                MonthlyBonus = user.StaffSalaryInfo?.MonthlyBonus ?? 0m,
                OtherDeductions = user.StaffSalaryInfo?.OtherDeductions ?? 0m,
                PendingRankTitle = user.StaffSalaryInfo?.PendingRankTitle,
                IsRankChangePending = user.StaffSalaryInfo?.IsRankChangePending ?? false,

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
            var username = User.Identity?.Name;
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (currentUser == null) return Unauthorized();

            // Ràng buộc bảo mật (ALT-2) - Bác sĩ/Lễ tân chỉ được xem hồ sơ bản thân
            if (currentUser.Role != "Admin" && currentUser.Id != id)
            {
                var detailsLog = new ActivityLog
                {
                    Username = username ?? "System",
                    Action = "Truy cập trái phép",
                    Details = $"Người dùng {username} cố gắng xem hồ sơ của nhân sự ID {id} trái phép."
                };
                _context.ActivityLogs.Add(detailsLog);
                await _context.SaveChangesAsync();
                return StatusCode(StatusCodes.Status403Forbidden);
            }

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
                
                // New profile fields
                DateOfBirth = profile?.DateOfBirth,
                Cccd = profile?.Cccd,
                CchnNumber = profile?.CchnNumber,
                CchnIssueDate = profile?.CchnIssueDate,
                CchnExpiryDate = profile?.CchnExpiryDate,
                CchnProvider = profile?.CchnProvider,
                AcademicRank = profile?.AcademicRank,
                AcademicDegree = profile?.AcademicDegree,
                JobRank = profile?.JobRank,
                ExperienceYears = profile?.ExperienceYears,

                Salary = new StaffSalaryInfoViewModel
                {
                    BaseSalary = salary?.BaseSalary ?? 0m,
                    DegreeMultiplier = degreeMultiplier,
                    DegreeTitle = salary?.DegreeTitle,
                    RankMultiplier = rankMultiplier,
                    RankTitle = salary?.RankTitle,
                    SpecializationAllowance = salary?.SpecializationAllowance ?? 0m,
                    SeniorityAllowance = salary?.SeniorityAllowance ?? 0m,
                    MonthlyBonus = salary?.MonthlyBonus ?? 0m,
                    OtherDeductions = salary?.OtherDeductions ?? 0m,
                    PendingRankTitle = salary?.PendingRankTitle,
                    IsRankChangePending = salary?.IsRankChangePending ?? false
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

        // GET: Staff/PersonalProfile
        [HttpGet]
        public async Task<IActionResult> PersonalProfile()
        {
            var username = User.Identity?.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToAction("Login", "Auth");
            return RedirectToAction(nameof(Details), new { id = user.Id });
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
                    MonthlyBonus = salary?.MonthlyBonus ?? 0m,
                    OtherDeductions = salary?.OtherDeductions ?? 0m,
                    PendingRankTitle = salary?.PendingRankTitle,
                    IsRankChangePending = salary?.IsRankChangePending ?? false
                },
                Qualifications = new List<StaffQualificationViewModel>()
            };

            return View(model);
        }

        // GET: Staff/SalaryList
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SalaryList()
        {
            var users = await _context.Users
                .Include(u => u.StaffProfile)
                .Include(u => u.StaffSalaryInfo)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var model = users.Select(user => {
                var profile = user.StaffProfile;
                var salary = user.StaffSalaryInfo;
                return new StaffProfileViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Role = user.Role,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    StaffCode = profile?.StaffCode ?? "",
                    PositionTitle = profile?.PositionTitle ?? "",
                    Department = profile?.Department ?? "",
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
                        MonthlyBonus = salary?.MonthlyBonus ?? 0m,
                        OtherDeductions = salary?.OtherDeductions ?? 0m,
                        PendingRankTitle = salary?.PendingRankTitle,
                        IsRankChangePending = salary?.IsRankChangePending ?? false
                    }
                };
            }).ToList();

            return View(model);
        }

        // POST: Staff/UpdateSalaryAdjustments
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSalaryAdjustments(int id, decimal monthlyBonus, decimal otherDeductions)
        {
            var salary = await _context.StaffSalaryInfos.FirstOrDefaultAsync(s => s.UserId == id);
            if (salary == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin lương." });
            }

            var oldBonus = salary.MonthlyBonus;
            var oldDeductions = salary.OtherDeductions;

            salary.MonthlyBonus = monthlyBonus;
            salary.OtherDeductions = otherDeductions;
            await _context.SaveChangesAsync();

            // Log activity
            var username = User.Identity?.Name ?? "System";
            var log = new ActivityLog
            {
                Username = username,
                Action = "Cập nhật nhanh lương",
                Details = $"Cập nhật nhanh lương cho nhân sự ID {id}: Thưởng KPI ({oldBonus:N0} -> {monthlyBonus:N0}đ), Khấu trừ ({oldDeductions:N0} -> {otherDeductions:N0}đ)"
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật thưởng và khấu trừ thành công!" });
        }

        // POST: Staff/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateStaffViewModel model)
        {
            if (ModelState.IsValid)
            {
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
        public async Task<IActionResult> Edit(EditStaffViewModel model)
        {
            var username = User.Identity?.Name;
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (currentUser == null) return Unauthorized();

            // Ràng buộc bảo mật (ALT-1) - Chỉ Admin mới được quyền sửa qua POST API
            if (currentUser.Role != "Admin")
            {
                var editPostLog = new ActivityLog
                {
                    Username = username ?? "System",
                    Action = "Chỉnh sửa trái phép",
                    Details = $"Người dùng {username} cố gắng chỉnh sửa trái phép hồ sơ của nhân sự ID {model.Id} qua POST API."
                };
                _context.ActivityLogs.Add(editPostLog);
                await _context.SaveChangesAsync();
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            // Ràng buộc ALT-3: Kiểm tra định dạng CCCD phải đúng 12 chữ số
            if (!string.IsNullOrEmpty(model.Cccd))
            {
                var cccdRegex = new System.Text.RegularExpressions.Regex(@"^\d{12}$");
                if (!cccdRegex.IsMatch(model.Cccd))
                {
                    ModelState.AddModelError("Cccd", "Số CCCD không hợp lệ — yêu cầu đúng 12 chữ số");
                }
            }

            // Ràng buộc ALT-4: Ngày hết hạn CCHN phải sau ngày cấp
            if (model.CchnExpiryDate.HasValue && model.CchnIssueDate.HasValue)
            {
                if (model.CchnExpiryDate.Value <= model.CchnIssueDate.Value)
                {
                    ModelState.AddModelError("CchnExpiryDate", "Ngày hết hạn phải sau ngày cấp");
                }
            }

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

            // Track changes for detailed audit log
            var changesList = new List<string>();

            if (user.FullName != model.FullName)
            {
                changesList.Add($"Họ tên: '{user.FullName}' -> '{model.FullName}'");
                user.FullName = model.FullName;
            }
            if (user.Email != model.Email)
            {
                changesList.Add($"Email: '{user.Email}' -> '{model.Email}'");
                user.Email = model.Email;
            }
            if (user.PhoneNumber != model.PhoneNumber)
            {
                changesList.Add($"Số điện thoại: '{user.PhoneNumber}' -> '{model.PhoneNumber}'");
                user.PhoneNumber = model.PhoneNumber;
            }
            
            var oldRole = user.Role;
            if (user.Role != model.Role)
            {
                changesList.Add($"Vai trò: '{user.Role}' -> '{model.Role}'");
                user.Role = model.Role;
            }

            var oldStatus = user.IsActive;
            if (user.IsActive != model.IsActive)
            {
                changesList.Add($"Trạng thái: '{(user.IsActive ? "Hoạt động" : "Tạm khóa")}' -> '{(model.IsActive ? "Hoạt động" : "Tạm khóa")}'");
                user.IsActive = model.IsActive;
            }

            if (user.StaffProfile == null)
            {
                user.StaffProfile = new StaffProfile();
                _context.StaffProfiles.Add(user.StaffProfile);
            }
            user.StaffProfile.UserId = user.Id;
            user.StaffProfile.User = user;

            // Profile detail checks
            if (user.StaffProfile.StaffCode != model.StaffCode)
            {
                changesList.Add($"Mã nhân viên: '{user.StaffProfile.StaffCode}' -> '{model.StaffCode}'");
                user.StaffProfile.StaffCode = model.StaffCode;
            }
            if (user.StaffProfile.PositionTitle != model.PositionTitle)
            {
                changesList.Add($"Chức vụ: '{user.StaffProfile.PositionTitle}' -> '{model.PositionTitle}'");
                user.StaffProfile.PositionTitle = model.PositionTitle;
            }
            if (user.StaffProfile.Department != model.Department)
            {
                changesList.Add($"Phòng ban: '{user.StaffProfile.Department}' -> '{model.Department}'");
                user.StaffProfile.Department = model.Department;
            }
            if (user.StaffProfile.Gender != model.Gender)
            {
                changesList.Add($"Giới tính: '{user.StaffProfile.Gender}' -> '{model.Gender}'");
                user.StaffProfile.Gender = model.Gender;
            }
            if (user.StaffProfile.Address != model.Address)
            {
                changesList.Add($"Địa chỉ: '{user.StaffProfile.Address}' -> '{model.Address}'");
                user.StaffProfile.Address = model.Address;
            }
            if (user.StaffProfile.JoinDate != model.JoinDate)
            {
                changesList.Add($"Ngày gia nhập: '{user.StaffProfile.JoinDate?.ToString("dd/MM/yyyy")}' -> '{model.JoinDate?.ToString("dd/MM/yyyy")}'");
                user.StaffProfile.JoinDate = model.JoinDate;
            }
            if (user.StaffProfile.PrimaryClinic != model.PrimaryClinic)
            {
                changesList.Add($"Phòng khám chính: '{user.StaffProfile.PrimaryClinic}' -> '{model.PrimaryClinic}'");
                user.StaffProfile.PrimaryClinic = model.PrimaryClinic;
            }

            // New profile fields checks
            if (user.StaffProfile.DateOfBirth != model.DateOfBirth)
            {
                changesList.Add($"Ngày sinh: '{user.StaffProfile.DateOfBirth?.ToString("dd/MM/yyyy")}' -> '{model.DateOfBirth?.ToString("dd/MM/yyyy")}'");
                user.StaffProfile.DateOfBirth = model.DateOfBirth;
            }
            if (user.StaffProfile.Cccd != model.Cccd)
            {
                changesList.Add($"CCCD: '{user.StaffProfile.Cccd}' -> '{model.Cccd}'");
                user.StaffProfile.Cccd = model.Cccd;
            }
            if (user.StaffProfile.CchnNumber != model.CchnNumber)
            {
                changesList.Add($"Số CCHN: '{user.StaffProfile.CchnNumber}' -> '{model.CchnNumber}'");
                user.StaffProfile.CchnNumber = model.CchnNumber;
            }
            if (user.StaffProfile.CchnIssueDate != model.CchnIssueDate)
            {
                changesList.Add($"Ngày cấp CCHN: '{user.StaffProfile.CchnIssueDate?.ToString("dd/MM/yyyy")}' -> '{model.CchnIssueDate?.ToString("dd/MM/yyyy")}'");
                user.StaffProfile.CchnIssueDate = model.CchnIssueDate;
            }
            if (user.StaffProfile.CchnExpiryDate != model.CchnExpiryDate)
            {
                changesList.Add($"Ngày hết hạn CCHN: '{user.StaffProfile.CchnExpiryDate?.ToString("dd/MM/yyyy")}' -> '{model.CchnExpiryDate?.ToString("dd/MM/yyyy")}'");
                user.StaffProfile.CchnExpiryDate = model.CchnExpiryDate;
            }
            if (user.StaffProfile.CchnProvider != model.CchnProvider)
            {
                changesList.Add($"Cơ quan cấp CCHN: '{user.StaffProfile.CchnProvider}' -> '{model.CchnProvider}'");
                user.StaffProfile.CchnProvider = model.CchnProvider;
            }
            if (user.StaffProfile.AcademicRank != model.AcademicRank)
            {
                changesList.Add($"Học hàm: '{user.StaffProfile.AcademicRank}' -> '{model.AcademicRank}'");
                user.StaffProfile.AcademicRank = model.AcademicRank;
            }
            if (user.StaffProfile.AcademicDegree != model.AcademicDegree)
            {
                changesList.Add($"Học vị: '{user.StaffProfile.AcademicDegree}' -> '{model.AcademicDegree}'");
                user.StaffProfile.AcademicDegree = model.AcademicDegree;
            }
            if (user.StaffProfile.ExperienceYears != model.ExperienceYears)
            {
                changesList.Add($"Thâm niên: '{user.StaffProfile.ExperienceYears}' -> '{model.ExperienceYears}'");
                user.StaffProfile.ExperienceYears = model.ExperienceYears;
            }

            // Ràng buộc ALT-6: Admin thay đổi hạng chức danh -> có hiệu lực từ kỳ lương tiếp theo (chốt không hồi tố)
            if (user.StaffProfile.JobRank != model.JobRank)
            {
                if (user.StaffSalaryInfo == null)
                {
                    user.StaffSalaryInfo = new StaffSalaryInfo { UserId = user.Id, User = user };
                    _context.StaffSalaryInfos.Add(user.StaffSalaryInfo);
                }
                
                user.StaffSalaryInfo.PendingRankTitle = model.JobRank;
                user.StaffSalaryInfo.IsRankChangePending = true;
                
                changesList.Add($"Hạng chức danh chờ hiệu lực (kỳ lương sau): '{user.StaffProfile.JobRank}' -> '{model.JobRank}'");
                // Giữ nguyên hạng chức danh hiện tại của hồ sơ ở kỳ lương hiện tại
            }

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
            user.StaffSalaryInfo.OtherDeductions = model.OtherDeductions ?? 0m;

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
                                changesList.Add($"Xóa bằng cấp: '{existing.Title}'");
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

                            changesList.Add($"Thêm bằng cấp mới: '{newQual.Title}'");
                            _context.StaffQualifications.Add(newQual);
                        }
                    }
                    else
                    {
                        var existing = user.StaffQualifications.FirstOrDefault(q => q.Id == qModel.Id);
                        if (existing != null)
                        {
                            if (existing.Title != qModel.Title || existing.Major != qModel.Major || existing.Institution != qModel.Institution || existing.Year != qModel.Year || existing.AcademicDegree != qModel.AcademicDegree || existing.AcademicRank != qModel.AcademicRank)
                            {
                                changesList.Add($"Cập nhật bằng cấp '{existing.Title}': đổi sang '{qModel.Title}', học vị '{qModel.AcademicDegree}', học hàm '{qModel.AcademicRank}'");
                            }
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

            // Ràng buộc ALT-5: Tạo cảnh báo hết hạn CCHN (gửi Admin/lưu log)
            if (model.CchnExpiryDate.HasValue)
            {
                var remainingDays = (model.CchnExpiryDate.Value.Date - DateTime.Today).Days;
                if (remainingDays >= 0 && remainingDays <= 30)
                {
                    var warningLog = new ActivityLog
                    {
                        Username = "System Alert",
                        Action = "Cảnh báo hết hạn CCHN",
                        Details = $"Chứng chỉ hành nghề của {user.FullName} sẽ hết hạn vào {model.CchnExpiryDate.Value.ToString("dd/MM/yyyy")} (còn {remainingDays} ngày)."
                    };
                    _context.ActivityLogs.Add(warningLog);
                    await _context.SaveChangesAsync();
                }
            }

            // Ghi nhận Audit Log chi tiết các thay đổi
            var details = $"Cập nhật hồ sơ của nhân sự: {user.FullName} ({user.Username}).";
            if (changesList.Any())
            {
                details += " Các trường thay đổi: " + string.Join(", ", changesList);
            }
            else
            {
                details += " Không có thay đổi dữ liệu chính.";
            }

            var log = new ActivityLog
            {
                Username = username ?? "System",
                Action = "Cập nhật hồ sơ",
                Details = details
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công";
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
