using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhakhoa.Data;
using Nhakhoa.Models;

namespace Nhakhoa.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
    public class PatientAccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PatientAccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /PatientAccount
        public async Task<IActionResult> Index(string? search, string? status, int page = 1)
        {
            int pageSize = 20;

            var query = _db.PatientAccounts
                .Include(pa => pa.Patient)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(pa =>
                    pa.FullName.ToLower().Contains(search) ||
                    pa.PhoneNumber.Contains(search) ||
                    (pa.Email != null && pa.Email.ToLower().Contains(search)));
            }

            if (status == "active")
                query = query.Where(pa => pa.IsActive);
            else if (status == "locked")
                query = query.Where(pa => !pa.IsActive);

            query = query.OrderByDescending(pa => pa.CreatedAt);

            int total = await query.CountAsync();
            var accounts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.Total = total;
            ViewBag.StatTotal = await _db.PatientAccounts.CountAsync();
            ViewBag.StatActive = await _db.PatientAccounts.CountAsync(pa => pa.IsActive);
            ViewBag.StatLocked = await _db.PatientAccounts.CountAsync(pa => !pa.IsActive);
            ViewBag.StatLinked = await _db.PatientAccounts.CountAsync(pa => pa.PatientId != null);

            return View(accounts);
        }

        // POST: /PatientAccount/Toggle
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id, string? returnUrl)
        {
            var account = await _db.PatientAccounts.FindAsync(id);
            if (account == null) return NotFound();

            account.IsActive = !account.IsActive;
            account.UpdatedAt = DateTime.Now;

            var action = account.IsActive ? "Mở khóa" : "Khóa";
            _db.ActivityLogs.Add(new ActivityLog
            {
                Username = User.Identity?.Name ?? "System",
                Action = $"{action} tài khoản Patient Portal",
                Details = $"Nhân viên {User.Identity?.Name} đã {action.ToLower()} tài khoản của bệnh nhân {account.FullName} (SĐT: {account.PhoneNumber}).",
                Timestamp = DateTime.Now
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã {action.ToLower()} tài khoản của {account.FullName}.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /PatientAccount/ResetPassword
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var account = await _db.PatientAccounts.Include(pa => pa.Patient).FirstOrDefaultAsync(pa => pa.Id == id);
            if (account == null) return NotFound();

            var tempPassword = $"BN@{account.Patient?.PatientCode ?? account.PhoneNumber}";
            account.PasswordHash = tempPassword;
            account.SecurityStamp = Guid.NewGuid().ToString();
            account.UpdatedAt = DateTime.Now;

            _db.ActivityLogs.Add(new ActivityLog
            {
                Username = User.Identity?.Name ?? "System",
                Action = "Đặt lại mật khẩu Patient Portal",
                Details = $"Nhân viên {User.Identity?.Name} đặt lại mật khẩu cho bệnh nhân {account.FullName}. MK tạm: {tempPassword}",
                Timestamp = DateTime.Now
            });

            await _db.SaveChangesAsync();
            TempData["PortalSuccess"] = $"Đã đặt lại mật khẩu cho <strong>{account.FullName}</strong>. Mật khẩu tạm thời: <strong>{tempPassword}</strong>";
            return RedirectToAction(nameof(Index));
        }

        // POST: /PatientAccount/Delete
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var account = await _db.PatientAccounts.FindAsync(id);
            if (account == null) return NotFound();

            _db.ActivityLogs.Add(new ActivityLog
            {
                Username = User.Identity?.Name ?? "System",
                Action = "Xóa tài khoản Patient Portal",
                Details = $"Admin {User.Identity?.Name} đã xóa tài khoản Patient Portal của bệnh nhân {account.FullName} (SĐT: {account.PhoneNumber}).",
                Timestamp = DateTime.Now
            });

            _db.PatientAccounts.Remove(account);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã xóa tài khoản của {account.FullName}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
