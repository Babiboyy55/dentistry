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
    public class ClinicController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClinicController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Clinic
        public async Task<IActionResult> Index(string search)
        {
            var query = _context.Clinics
                .Include(c => c.DefaultSpecialty)
                .Include(c => c.Shifts)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Name.Contains(search) || c.Location.Contains(search));
            }

            var clinics = await query.OrderBy(c => c.Id).ToListAsync();

            // Stats
            var allClinics = await _context.Clinics.ToListAsync();
            ViewBag.TotalClinics = allClinics.Count;
            ViewBag.ActiveClinics = allClinics.Count(c => c.IsActive);
            ViewBag.TotalCapacity = allClinics.Sum(c => c.Capacity);

            // Fetch Specialties for creation panel dropdown
            ViewBag.Specialties = await _context.Specialties.OrderBy(s => s.Name).ToListAsync();

            // Clinic action logs
            ViewBag.AuditLogs = await _context.ActivityLogs
                .Where(l => l.Action.Contains("phòng khám") || l.Action.Contains("Phòng khám"))
                .OrderByDescending(l => l.Timestamp)
                .Take(6)
                .ToListAsync();

            ViewBag.CurrentSearch = search;

            // Seed sample future shifts if empty, to aid debugging/testing
            var today = DateTime.Today;
            var futureShiftsExist = await _context.Shifts.AnyAsync(s => s.ShiftDate >= today);
            if (!futureShiftsExist && allClinics.Any())
            {
                var doc = await _context.StaffProfiles.FirstOrDefaultAsync();
                if (doc != null)
                {
                    _context.Shifts.Add(new Shift
                    {
                        ClinicId = allClinics.First().Id,
                        StaffProfileId = doc.Id,
                        ShiftDate = DateTime.Today.AddDays(5),
                        IsActive = true
                    });
                    await _context.SaveChangesAsync();
                }
            }

            return View(clinics);
        }

        // GET: Clinic/DetailsJson/5
        [HttpGet]
        public async Task<IActionResult> DetailsJson(int id)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null)
            {
                return NotFound();
            }
            return Json(new
            {
                id = clinic.Id,
                name = clinic.Name,
                location = clinic.Location,
                defaultSpecialtyId = clinic.DefaultSpecialtyId,
                capacity = clinic.Capacity,
                isActive = clinic.IsActive,
                updatedAt = clinic.UpdatedAt.ToString("dd/MM/yyyy")
            });
        }

        // POST: Clinic/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Save(int id, string name, string location, int? defaultSpecialtyId, int capacity, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "Tên phòng khám không được để trống.";
                return RedirectToAction(nameof(Index));
            }

            // Clean inputs
            name = name.Trim();
            location = location?.Trim() ?? "";

            // Validate capacity is a positive integer (ALT-6)
            if (capacity <= 0)
            {
                TempData["ErrorMessage"] = "Sức chứa phòng khám phải là số nguyên dương (lớn hơn 0).";
                return RedirectToAction(nameof(Index));
            }

            // Check duplicate name (excluding current id)
            var duplicateExists = await _context.Clinics
                .AnyAsync(c => c.Id != id && c.Name.ToLower() == name.ToLower());

            if (duplicateExists)
            {
                TempData["ErrorMessage"] = "Tên phòng khám đã tồn tại trong hệ thống.";
                return RedirectToAction(nameof(Index));
            }

            // Check that default specialty exists
            if (defaultSpecialtyId.HasValue)
            {
                var specExists = await _context.Specialties.AnyAsync(s => s.Id == defaultSpecialtyId.Value);
                if (!specExists)
                {
                    TempData["ErrorMessage"] = "Chuyên khoa mặc định không tồn tại hoặc đã bị vô hiệu hóa.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var currentUser = User.Identity?.Name ?? "Admin System";
            string actionDetail = "";

            if (id == 0) // Create new
            {
                var clinic = new Clinic
                {
                    Name = name,
                    Location = location,
                    DefaultSpecialtyId = defaultSpecialtyId,
                    Capacity = capacity,
                    IsActive = isActive,
                    UpdatedAt = DateTime.Now
                };
                _context.Clinics.Add(clinic);
                actionDetail = $"Thêm mới phòng khám: {name} (Vị trí: {location}, Sức chứa: {capacity})";
            }
            else // Edit
            {
                var clinic = await _context.Clinics.FindAsync(id);
                if (clinic == null)
                {
                    return NotFound();
                }

                var oldName = clinic.Name;
                var oldLoc = clinic.Location;
                var oldCap = clinic.Capacity;
                var oldStatus = clinic.IsActive;

                clinic.Name = name;
                clinic.Location = location;
                clinic.DefaultSpecialtyId = defaultSpecialtyId;
                clinic.Capacity = capacity;
                clinic.IsActive = isActive;
                clinic.UpdatedAt = DateTime.Now;

                _context.Clinics.Update(clinic);

                actionDetail = $"Cập nhật phòng khám: {name}.";
                if (oldName != name) actionDetail += $" Đổi tên: {oldName} -> {name}.";
                if (oldLoc != location) actionDetail += $" Đổi vị trí: {oldLoc} -> {location}.";
                if (oldCap != capacity) actionDetail += $" Đổi sức chứa: {oldCap} -> {capacity}.";
                if (oldStatus != isActive) actionDetail += $" Đổi trạng thái: {(oldStatus ? "Hoạt động" : "Ẩn")} -> {(isActive ? "Hoạt động" : "Ẩn")}.";
            }

            await _context.SaveChangesAsync();

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Username = currentUser,
                Action = id == 0 ? "Thêm phòng khám" : "Cập nhật phòng khám",
                Details = actionDetail
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = id == 0 ? "Thêm phòng khám thành công" : "Cập nhật phòng khám thành công";
            return RedirectToAction(nameof(Index));
        }

        // GET: Clinic/GetDeleteConstraints/5
        [HttpGet]
        public async Task<IActionResult> GetDeleteConstraints(int id)
        {
            var clinic = await _context.Clinics
                .Include(c => c.Shifts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (clinic == null)
            {
                return NotFound();
            }

            // Filter upcoming shifts in the future (shift date is today or later)
            var today = DateTime.Today;
            var futureShiftsCount = clinic.Shifts.Count(s => s.ShiftDate >= today && s.IsActive);

            return Json(new
            {
                hasConstraints = futureShiftsCount > 0,
                clinicName = clinic.Name,
                futureShifts = futureShiftsCount
            });
        }

        // POST: Clinic/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var clinic = await _context.Clinics
                .Include(c => c.Shifts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (clinic == null)
            {
                return NotFound();
            }

            // Check upcoming future shifts (ALT-5)
            var today = DateTime.Today;
            var futureShiftsCount = clinic.Shifts.Count(s => s.ShiftDate >= today && s.IsActive);

            if (futureShiftsCount > 0)
            {
                TempData["ErrorMessage"] = $"Phòng khám có {futureShiftsCount} ca trực trong tương lai. Vui lòng hủy hoặc chuyển ca trực trước khi xóa phòng.";
                return RedirectToAction(nameof(Index));
            }

            // No constraints - delete physical room and any historical completed shifts
            _context.Clinics.Remove(clinic);

            // Log activity
            var currentUser = User.Identity?.Name ?? "Admin System";
            _context.ActivityLogs.Add(new ActivityLog
            {
                Username = currentUser,
                Action = "Xóa phòng khám",
                Details = $"Đã xóa phòng khám: {clinic.Name} (Vị trí: {clinic.Location})"
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa phòng khám thành công";
            return RedirectToAction(nameof(Index));
        }
    }
}
