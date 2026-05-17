using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhakhoa.Data;
using Nhakhoa.Models;
using Nhakhoa.ViewModels;

namespace Nhakhoa.Controllers
{
    [Authorize(Roles = "Admin")]
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
        public IActionResult Create()
        {
            return View();
        }

        // GET: Staff/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
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
                IsActive = user.IsActive
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
                        Category = q.Category
                    })
                    .ToList()
            };

            return View(model);
        }

        // POST: Staff/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    IsActive = true
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
                return RedirectToAction(nameof(Create));
            }

            return View(model);
        }

        // POST: Staff/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditStaffViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.Id);
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

            await _context.SaveChangesAsync();

            // Lưu lịch sử hoạt động
            var currentUser = User.Identity?.Name ?? "System";
            var details = $"Cập nhật thông tin nhân sự: {user.FullName} ({user.Username}).";
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

            TempData["SuccessMessage"] = "Cập nhật thông tin nhân sự thành công";
            return RedirectToAction(nameof(Index));
        }

        // POST: Staff/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var defaultPassword = "123456";
            user.PasswordHash = defaultPassword;
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
        public async Task<IActionResult> ActivityLog()
        {
            var logs = await _context.ActivityLogs
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();

            return View(logs);
        }
    }
}
