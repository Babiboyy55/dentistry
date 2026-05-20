using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhakhoa.Data;
using Nhakhoa.Models;
using System.Security.Claims;

namespace Nhakhoa.Controllers
{
    public class RolePermissionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RolePermissionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /RolePermission
        public async Task<IActionResult> Index()
        {
            if (!User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Login", "Auth");

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin")
                return StatusCode(403);

            var allPerms = await _context.RolePermissions
                .OrderBy(p => p.SortOrder)
                .ToListAsync();

            // Recent audit logs about permission changes
            var logs = await _context.ActivityLogs
                .Where(l => l.Action == "Cập nhật phân quyền" || l.Action == "Khôi phục phân quyền mặc định")
                .OrderByDescending(l => l.Timestamp)
                .Take(6)
                .ToListAsync();

            ViewBag.Permissions = allPerms;
            ViewBag.AuditLogs = logs;
            return View();
        }

        // POST: /RolePermission/Save
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SavePermissionsRequest request)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Admin")
                return StatusCode(403);

            if (request?.Role == null || request.Permissions == null)
                return BadRequest("Dữ liệu không hợp lệ.");

            var existingPerms = await _context.RolePermissions
                .Where(p => p.Role == request.Role)
                .ToListAsync();

            var changes = new List<string>();

            foreach (var perm in existingPerms)
            {
                if (request.Permissions.TryGetValue(perm.ModuleKey, out bool newValue))
                {
                    if (perm.IsAllowed != newValue)
                    {
                        changes.Add($"{(newValue ? "+" : "-")} {perm.ModuleName}");
                        perm.IsAllowed = newValue;
                    }
                }
            }

            if (changes.Count > 0)
            {
                _context.RolePermissions.UpdateRange(existingPerms);

                var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
                _context.ActivityLogs.Add(new ActivityLog
                {
                    Username = username,
                    Action = "Cập nhật phân quyền",
                    Details = $"Vai trò [{request.Role}] — {string.Join(", ", changes)}"
                });

                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, changed = changes.Count });
        }

        // POST: /RolePermission/Reset
        [HttpPost]
        public async Task<IActionResult> Reset([FromBody] ResetPermissionsRequest request)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != "Admin")
                return StatusCode(403);

            if (string.IsNullOrEmpty(request?.Role))
                return BadRequest();

            // Default permission matrix
            var defaults = new Dictionary<string, HashSet<string>>
            {
                ["Admin"]        = new() { "account_rbac","audit_log","system_config","emr","prescription","lab_test","schedule_view","appointment","patient_reg","invoice","patient_admin" },
                ["Doctor"]       = new() { "emr","prescription","lab_test","schedule_view" },
                ["Receptionist"] = new() { "appointment","patient_reg","invoice","patient_admin" },
            };

            if (!defaults.ContainsKey(request.Role))
                return BadRequest("Vai trò không tồn tại.");

            var perms = await _context.RolePermissions
                .Where(p => p.Role == request.Role)
                .ToListAsync();

            foreach (var perm in perms)
                perm.IsAllowed = defaults[request.Role].Contains(perm.ModuleKey);

            _context.RolePermissions.UpdateRange(perms);

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            _context.ActivityLogs.Add(new ActivityLog
            {
                Username = username,
                Action = "Khôi phục phân quyền mặc định",
                Details = $"Đã khôi phục phân quyền mặc định cho vai trò [{request.Role}]."
            });

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }

    public class SavePermissionsRequest
    {
        public string Role { get; set; } = string.Empty;
        public Dictionary<string, bool> Permissions { get; set; } = new();
    }

    public class ResetPermissionsRequest
    {
        public string Role { get; set; } = string.Empty;
    }
}
