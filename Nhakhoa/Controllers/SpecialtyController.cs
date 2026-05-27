using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhakhoa.Data;
using Nhakhoa.Models;

namespace Nhakhoa.Controllers
{
    [Authorize]
    public class SpecialtyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SpecialtyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Specialty
        public async Task<IActionResult> Index(string search)
        {
            var query = _context.Specialties
                .Include(s => s.DoctorSpecialties)
                    .ThenInclude(ds => ds.StaffProfile)
                        .ThenInclude(sp => sp.User)
                .Include(s => s.MedicalServices)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => s.Name.Contains(search) || s.Code.Contains(search));
            }

            var specialties = await query.OrderBy(s => s.Code).ToListAsync();

            // Stats
            var allSpecialties = await _context.Specialties.ToListAsync();
            ViewBag.TotalSpecialties = allSpecialties.Count;
            ViewBag.TotalDoctorsLinked = await _context.DoctorSpecialties.CountAsync();
            ViewBag.TotalActiveSpecialties = allSpecialties.Count; // Specialties don't have IsActive, but we can count ones that have active doctors
            
            // Get audit logs related to Specialty
            ViewBag.AuditLogs = await _context.ActivityLogs
                .Where(l => l.Action.Contains("chuyên khoa") || l.Action.Contains("Chuyên khoa") || l.Action.Contains("Bác sĩ"))
                .OrderByDescending(l => l.Timestamp)
                .Take(6)
                .ToListAsync();

            ViewBag.CurrentSearch = search;

            return View(specialties);
        }

        // GET: Specialty/DetailsJson/5
        [HttpGet]
        public async Task<IActionResult> DetailsJson(int id)
        {
            var specialty = await _context.Specialties.FindAsync(id);
            if (specialty == null)
            {
                return NotFound();
            }
            return Json(new
            {
                id = specialty.Id,
                name = specialty.Name,
                code = specialty.Code,
                description = specialty.Description,
                updatedAt = specialty.UpdatedAt.ToString("dd/MM/yyyy")
            });
        }

        // POST: Specialty/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Save(int id, string name, string code, string description)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(code))
            {
                TempData["ErrorMessage"] = "Tên và Mã chuyên khoa không được để trống.";
                return RedirectToAction(nameof(Index));
            }

            // Clean inputs
            name = name.Trim();
            code = code.Trim().ToUpper();
            description = description?.Trim() ?? "";

            // Check duplicate name or code (ALT-1)
            var duplicateExists = await _context.Specialties
                .AnyAsync(s => s.Id != id && (s.Name.ToLower() == name.ToLower() || s.Code.ToLower() == code.ToLower()));

            if (duplicateExists)
            {
                TempData["ErrorMessage"] = "Tên hoặc Mã chuyên khoa đã tồn tại trong hệ thống.";
                return RedirectToAction(nameof(Index));
            }

            var currentUser = User.Identity?.Name ?? "Admin System";
            string actionDetail = "";

            if (id == 0) // Add new
            {
                var specialty = new Specialty
                {
                    Name = name,
                    Code = code,
                    Description = description,
                    UpdatedAt = DateTime.Now
                };
                _context.Specialties.Add(specialty);
                actionDetail = $"Thêm mới chuyên khoa: {name} (Mã: {code})";
            }
            else // Edit
            {
                var specialty = await _context.Specialties.FindAsync(id);
                if (specialty == null)
                {
                    return NotFound();
                }

                var oldName = specialty.Name;
                var oldCode = specialty.Code;
                var oldDesc = specialty.Description;

                specialty.Name = name;
                specialty.Code = code;
                specialty.Description = description;
                specialty.UpdatedAt = DateTime.Now;

                _context.Specialties.Update(specialty);

                actionDetail = $"Cập nhật chuyên khoa: {name} (Mã: {code}).";
                if (oldName != name) actionDetail += $" Đổi tên: {oldName} -> {name}.";
                if (oldCode != code) actionDetail += $" Đổi mã: {oldCode} -> {code}.";
            }

            await _context.SaveChangesAsync();

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Username = currentUser,
                Action = id == 0 ? "Thêm chuyên khoa" : "Cập nhật chuyên khoa",
                Details = actionDetail
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = id == 0 ? "Thêm chuyên khoa thành công" : "Cập nhật chuyên khoa thành công";
            return RedirectToAction(nameof(Index));
        }

        // GET: Specialty/GetDeleteConstraints/5
        [HttpGet]
        public async Task<IActionResult> GetDeleteConstraints(int id)
        {
            var specialty = await _context.Specialties
                .Include(s => s.DoctorSpecialties)
                .Include(s => s.MedicalServices)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (specialty == null)
            {
                return NotFound();
            }

            return Json(new
            {
                hasConstraints = specialty.DoctorSpecialties.Any() || specialty.MedicalServices.Any(),
                specialtyName = specialty.Name,
                doctorCount = specialty.DoctorSpecialties.Count,
                serviceCount = specialty.MedicalServices.Count
            });
        }

        // POST: Specialty/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var specialty = await _context.Specialties
                .Include(s => s.DoctorSpecialties)
                .Include(s => s.MedicalServices)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (specialty == null)
            {
                return NotFound();
            }

            // Guard constraints (ALT-2, ALT-3)
            if (specialty.DoctorSpecialties.Any())
            {
                TempData["ErrorMessage"] = $"Chuyên khoa còn {specialty.DoctorSpecialties.Count} bác sĩ đang gán. Vui lòng chuyển hoặc bỏ gán bác sĩ trước khi xóa.";
                return RedirectToAction(nameof(Index));
            }

            if (specialty.MedicalServices.Any())
            {
                TempData["ErrorMessage"] = $"Chuyên khoa còn {specialty.MedicalServices.Count} dịch vụ đang liên kết. Vui lòng cập nhật chuyên khoa của các dịch vụ đó trước.";
                return RedirectToAction(nameof(Index));
            }

            _context.Specialties.Remove(specialty);

            // Log activity
            var currentUser = User.Identity?.Name ?? "Admin System";
            _context.ActivityLogs.Add(new ActivityLog
            {
                Username = currentUser,
                Action = "Xóa chuyên khoa",
                Details = $"Đã xóa chuyên khoa: {specialty.Name} (Mã: {specialty.Code})"
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa chuyên khoa thành công";
            return RedirectToAction(nameof(Index));
        }

        // GET: Specialty/Doctors/5
        public async Task<IActionResult> Doctors(int id)
        {
            var specialty = await _context.Specialties
                .Include(s => s.DoctorSpecialties)
                    .ThenInclude(ds => ds.StaffProfile)
                        .ThenInclude(sp => sp.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (specialty == null)
            {
                return NotFound();
            }

            // Find all doctor users
            var allDoctors = await _context.StaffProfiles
                .Include(sp => sp.User)
                .Where(sp => sp.User.Role == "Doctor")
                .ToListAsync();

            // Assigned Doctors
            var assignedDoctorIds = specialty.DoctorSpecialties.Select(ds => ds.StaffProfileId).ToHashSet();
            
            ViewBag.AssignedDoctors = allDoctors
                .Where(d => assignedDoctorIds.Contains(d.Id))
                .ToList();

            ViewBag.AvailableDoctors = allDoctors
                .Where(d => !assignedDoctorIds.Contains(d.Id))
                .ToList();

            // Logs specific to doctor assignment for this specialty
            ViewBag.AuditLogs = await _context.ActivityLogs
                .Where(l => l.Details.Contains(specialty.Name) && (l.Action.Contains("gán") || l.Action.Contains("bỏ gán") || l.Action.Contains("Bác sĩ")))
                .OrderByDescending(l => l.Timestamp)
                .Take(6)
                .ToListAsync();

            return View(specialty);
        }

        // POST: Specialty/SaveDoctorAssignments
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SaveDoctorAssignments(int specialtyId, int[] assignedDoctorIds)
        {
            var specialty = await _context.Specialties
                .Include(s => s.DoctorSpecialties)
                    .ThenInclude(ds => ds.StaffProfile)
                        .ThenInclude(sp => sp.User)
                .FirstOrDefaultAsync(s => s.Id == specialtyId);

            if (specialty == null)
            {
                return NotFound();
            }

            // Verify active status of all assigned doctors (ALT-4)
            foreach (var docId in assignedDoctorIds)
            {
                var staff = await _context.StaffProfiles
                    .Include(sp => sp.User)
                    .FirstOrDefaultAsync(sp => sp.Id == docId);

                if (staff != null && !staff.User.IsActive)
                {
                    TempData["ErrorMessage"] = $"Bác sĩ {staff.User.FullName} hiện không hoạt động, không thể gán vào chuyên khoa.";
                    return RedirectToAction(nameof(Doctors), new { id = specialtyId });
                }
            }

            var originalIds = specialty.DoctorSpecialties.Select(ds => ds.StaffProfileId).ToList();
            var newIds = assignedDoctorIds.ToList();

            var toAdd = newIds.Except(originalIds).ToList();
            var toRemove = originalIds.Except(newIds).ToList();

            var currentUser = User.Identity?.Name ?? "Admin System";
            var logDetails = new List<string>();

            // Process removals
            foreach (var removeId in toRemove)
            {
                var ds = specialty.DoctorSpecialties.FirstOrDefault(d => d.StaffProfileId == removeId);
                if (ds != null)
                {
                    _context.DoctorSpecialties.Remove(ds);
                    var doc = await _context.StaffProfiles.Include(sp => sp.User).FirstOrDefaultAsync(sp => sp.Id == removeId);
                    logDetails.Add($"Bỏ gán BS. {doc?.User?.FullName ?? "N/A"} khỏi chuyên khoa {specialty.Name}");
                }
            }

            // Process additions
            foreach (var addId in toAdd)
            {
                var ds = new DoctorSpecialty
                {
                    SpecialtyId = specialtyId,
                    StaffProfileId = addId
                };
                _context.DoctorSpecialties.Add(ds);
                var doc = await _context.StaffProfiles.Include(sp => sp.User).FirstOrDefaultAsync(sp => sp.Id == addId);
                logDetails.Add($"Gán BS. {doc?.User?.FullName ?? "N/A"} vào chuyên khoa {specialty.Name}");
            }

            if (logDetails.Count > 0)
            {
                await _context.SaveChangesAsync();

                // Log audit
                foreach (var detail in logDetails)
                {
                    _context.ActivityLogs.Add(new ActivityLog
                    {
                        Username = currentUser,
                        Action = detail.StartsWith("Gán") ? "Gán bác sĩ vào chuyên khoa" : "Bỏ gán bác sĩ",
                        Details = detail
                    });
                }
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Cập nhật danh sách bác sĩ thành công";
            return RedirectToAction(nameof(Doctors), new { id = specialtyId });
        }
    }
}
