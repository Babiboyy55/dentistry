using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhakhoa.Data;
using Nhakhoa.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nhakhoa.Controllers
{
    [Authorize]
    public class ScheduleController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ScheduleController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Schedule — Doctor's personal schedule or Admin's overview
        public async Task<IActionResult> Index(int? month, int? year)
        {
            int m = month ?? DateTime.Now.Month;
            int y = year ?? DateTime.Now.Year;

            var startDate = new DateTime(y, m, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            IQueryable<Shift> shiftQuery = _db.Shifts
                .Include(s => s.StaffProfile).ThenInclude(sp => sp.User)
                .Include(s => s.Clinic)
                .Where(s => s.ShiftDate >= startDate && s.ShiftDate <= endDate && s.IsActive);

            // Doctors only see their own shifts
            if (User.IsInRole("Doctor"))
            {
                var staffProfile = await _db.StaffProfiles
                    .FirstOrDefaultAsync(sp => sp.User!.Username == User.Identity!.Name);
                if (staffProfile != null)
                    shiftQuery = shiftQuery.Where(s => s.StaffProfileId == staffProfile.Id);
            }

            var shifts = await shiftQuery.ToListAsync();
            var holidays = await _db.HolidayDates
                .Where(h => h.Date >= startDate && h.Date <= endDate)
                .ToListAsync();

            var settings = await _db.ShiftSettings.ToListAsync();

            ViewBag.Month = m;
            ViewBag.Year = y;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.Holidays = holidays;
            ViewBag.ShiftSettings = settings;
            ViewBag.Clinics = await _db.Clinics.Where(c => c.IsActive).ToListAsync();
            ViewBag.Doctors = await _db.StaffProfiles
                .Include(sp => sp.User)
                .Where(sp => sp.User!.IsActive && sp.User.Role == "Doctor")
                .ToListAsync();

            return View(shifts);
        }

        // POST: /Schedule/UpdateSettings
        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSettings(int id, string shiftName, string startTime, string endTime, double durationHours, int maxShiftsPerWeek)
        {
            var setting = await _db.ShiftSettings.FindAsync(id);
            if (setting == null) return NotFound();

            setting.ShiftName = shiftName;
            setting.StartTime = startTime;
            setting.EndTime = endTime;
            setting.DurationHours = durationHours;
            setting.MaxShiftsPerWeek = maxShiftsPerWeek;

            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã cập nhật cấu hình ca {setting.ShiftName}!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Schedule/AddShiftSetting
        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddShiftSetting(string shiftName, string startTime, string endTime, double durationHours, int maxShiftsPerWeek)
        {
            if (string.IsNullOrWhiteSpace(shiftName) || string.IsNullOrWhiteSpace(startTime) || string.IsNullOrWhiteSpace(endTime))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin ca!";
                return RedirectToAction(nameof(Index));
            }

            var setting = new ShiftSetting
            {
                ShiftName = shiftName.Trim(),
                StartTime = startTime.Trim(),
                EndTime = endTime.Trim(),
                DurationHours = durationHours,
                MaxShiftsPerWeek = maxShiftsPerWeek
            };

            _db.ShiftSettings.Add(setting);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã thêm ca mới: {setting.ShiftName}!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Schedule/DeleteShiftSetting/5
        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteShiftSetting(int id)
        {
            var setting = await _db.ShiftSettings.FindAsync(id);
            if (setting == null) return NotFound();

            // Check if any shift is using this type name
            var inUse = await _db.Shifts.AnyAsync(s => s.ShiftType == setting.ShiftName && s.IsActive);
            if (inUse)
            {
                TempData["Error"] = $"Không thể xóa ca {setting.ShiftName} vì đang có lịch trực đăng ký theo ca này.";
                return RedirectToAction(nameof(Index));
            }

            _db.ShiftSettings.Remove(setting);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã xóa cấu hình ca {setting.ShiftName}.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Schedule/RegisterShift
        [HttpPost]
        public async Task<IActionResult> RegisterShift(int clinicId, DateTime shiftDate, string shiftType)
        {
            // Get current doctor's staff profile
            var staffProfile = await _db.StaffProfiles
                .FirstOrDefaultAsync(sp => sp.User!.Username == User.Identity!.Name);

            if (staffProfile == null)
                return BadRequest(new { error = "Không tìm thấy hồ sơ nhân sự." });

            // Check if day is a holiday
            var holiday = await _db.HolidayDates.FirstOrDefaultAsync(h => h.Date.Date == shiftDate.Date);
            if (holiday != null)
                return BadRequest(new { error = $"Không thể đăng ký ca vào ngày nghỉ: {holiday.Name}." });

            // Check duplicate
            var duplicate = await _db.Shifts.AnyAsync(s =>
                s.StaffProfileId == staffProfile.Id &&
                s.ShiftDate.Date == shiftDate.Date &&
                s.ShiftType == shiftType &&
                s.IsActive);

            if (duplicate)
                return BadRequest(new { error = "Bạn đã đăng ký ca này rồi." });

            // Validate weekly shift limit
            int diff = (7 + (int)shiftDate.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            DateTime weekStart = shiftDate.AddDays(-1 * diff).Date;
            DateTime weekEnd = weekStart.AddDays(6).Date;

            var currentShiftsThisWeek = await _db.Shifts.CountAsync(s =>
                s.StaffProfileId == staffProfile.Id &&
                s.ShiftDate.Date >= weekStart && s.ShiftDate.Date <= weekEnd &&
                s.ShiftType == shiftType &&
                s.IsActive);

            var limitSetting = await _db.ShiftSettings.FirstOrDefaultAsync(s => s.ShiftName == shiftType);
            if (limitSetting != null && currentShiftsThisWeek >= limitSetting.MaxShiftsPerWeek)
            {
                return BadRequest(new { error = $"Bạn đã vượt quá giới hạn số ca trực cho ca '{shiftType}' trong tuần này (Tối đa: {limitSetting.MaxShiftsPerWeek} ca)." });
            }

            var shift = new Shift
            {
                ClinicId = clinicId,
                StaffProfileId = staffProfile.Id,
                ShiftDate = shiftDate.Date,
                ShiftType = shiftType,
                IsActive = true,
                RegisteredBy = User.Identity?.Name,
                CreatedAt = DateTime.Now
            };

            _db.Shifts.Add(shift);
            await _db.SaveChangesAsync();
            return Ok(new { success = true, shiftId = shift.Id });
        }

        // POST: /Schedule/CancelShift/5
        [HttpPost]
        public async Task<IActionResult> CancelShift(int id)
        {
            var shift = await _db.Shifts.FindAsync(id);
            if (shift == null) return NotFound();

            // Check if shift has appointments
            var hasAppointments = await _db.Appointments.AnyAsync(a =>
                a.StaffProfileId == shift.StaffProfileId &&
                a.AppointmentDate.Date == shift.ShiftDate.Date &&
                a.Session == shift.ShiftType &&
                a.Status != "Đã hủy");

            if (hasAppointments)
            {
                var count = await _db.Appointments.CountAsync(a =>
                    a.StaffProfileId == shift.StaffProfileId &&
                    a.AppointmentDate.Date == shift.ShiftDate.Date &&
                    a.Session == shift.ShiftType &&
                    a.Status != "Đã hủy");
                return BadRequest(new { error = $"Ca này đang có {count} lịch hẹn. Vui lòng xử lý lịch hẹn trước khi hủy ca." });
            }

            // Doctors can only cancel their own shifts; Admins can cancel any
            if (User.IsInRole("Doctor"))
            {
                var staffProfile = await _db.StaffProfiles
                    .FirstOrDefaultAsync(sp => sp.User!.Username == User.Identity!.Name);
                if (staffProfile == null || shift.StaffProfileId != staffProfile.Id)
                    return Forbid();
            }

            shift.IsActive = false;
            await _db.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // POST: /Schedule/RegisterShiftForDoctor (Admin only)
        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterShiftForDoctor(int staffProfileId, int clinicId, DateTime shiftDate, string shiftType)
        {
            var holiday = await _db.HolidayDates.FirstOrDefaultAsync(h => h.Date.Date == shiftDate.Date);
            if (holiday != null)
                return BadRequest(new { error = $"Không thể đặt ca vào ngày nghỉ: {holiday.Name}." });

            var duplicate = await _db.Shifts.AnyAsync(s =>
                s.StaffProfileId == staffProfileId &&
                s.ShiftDate.Date == shiftDate.Date &&
                s.ShiftType == shiftType &&
                s.IsActive);

            if (duplicate)
                return BadRequest(new { error = "Bác sĩ này đã có ca trong buổi đó." });

            // Validate weekly shift limit
            int diff = (7 + (int)shiftDate.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            DateTime weekStart = shiftDate.AddDays(-1 * diff).Date;
            DateTime weekEnd = weekStart.AddDays(6).Date;

            var currentShiftsThisWeek = await _db.Shifts.CountAsync(s =>
                s.StaffProfileId == staffProfileId &&
                s.ShiftDate.Date >= weekStart && s.ShiftDate.Date <= weekEnd &&
                s.ShiftType == shiftType &&
                s.IsActive);

            var limitSetting = await _db.ShiftSettings.FirstOrDefaultAsync(s => s.ShiftName == shiftType);
            if (limitSetting != null && currentShiftsThisWeek >= limitSetting.MaxShiftsPerWeek)
            {
                return BadRequest(new { error = $"Bác sĩ này đã vượt quá giới hạn số ca trực cho ca '{shiftType}' trong tuần này (Tối đa: {limitSetting.MaxShiftsPerWeek} ca)." });
            }

            var shift = new Shift
            {
                ClinicId = clinicId,
                StaffProfileId = staffProfileId,
                ShiftDate = shiftDate.Date,
                ShiftType = shiftType,
                IsActive = true,
                RegisteredBy = $"Admin ({User.Identity?.Name})",
                CreatedAt = DateTime.Now
            };

            _db.Shifts.Add(shift);
            await _db.SaveChangesAsync();
            return Ok(new { success = true, shiftId = shift.Id });
        }

        // GET: /Schedule/Holidays
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Holidays()
        {
            var holidays = await _db.HolidayDates
                .OrderBy(h => h.Date)
                .ToListAsync();
            return View(holidays);
        }

        // POST: /Schedule/AddHoliday
        [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHoliday(HolidayDate model)
        {
            // Check for conflicting shifts
            var affectedShifts = await _db.Shifts
                .Include(s => s.StaffProfile).ThenInclude(sp => sp.User)
                .Where(s => s.ShiftDate.Date == model.Date.Date && s.IsActive)
                .ToListAsync();

            if (affectedShifts.Any() && Request.Form["Confirmed"].FirstOrDefault() != "true")
            {
                // Return JSON with affected info for confirmation
                var names = affectedShifts.Select(s => s.StaffProfile?.User?.FullName ?? "—").Distinct().ToList();
                return Json(new
                {
                    requireConfirm = true,
                    affectedCount = affectedShifts.Count,
                    affectedDoctors = names
                });
            }

            model.CreatedAt = DateTime.Now;
            model.CreatedBy = User.Identity?.Name;
            _db.HolidayDates.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Đã thêm ngày nghỉ: {model.Name}";
            return RedirectToAction(nameof(Holidays));
        }

        // POST: /Schedule/DeleteHoliday/5
        [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHoliday(int id)
        {
            var holiday = await _db.HolidayDates.FindAsync(id);
            if (holiday == null) return NotFound();

            _db.HolidayDates.Remove(holiday);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa ngày nghỉ.";
            return RedirectToAction(nameof(Holidays));
        }

        // GET: /Schedule/GetShifts (AJAX — calendar data)
        [HttpGet]
        public async Task<IActionResult> GetShifts(int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            IQueryable<Shift> query = _db.Shifts
                .Include(s => s.StaffProfile).ThenInclude(sp => sp.User)
                .Include(s => s.Clinic)
                .Where(s => s.ShiftDate >= startDate && s.ShiftDate <= endDate && s.IsActive);

            if (User.IsInRole("Doctor"))
            {
                var profile = await _db.StaffProfiles.FirstOrDefaultAsync(sp => sp.User!.Username == User.Identity!.Name);
                if (profile != null)
                    query = query.Where(s => s.StaffProfileId == profile.Id);
            }

            var shifts = await query.Select(s => new
            {
                s.Id,
                s.ShiftDate,
                s.ShiftType,
                s.StaffProfileId,
                DoctorName = s.StaffProfile.User!.FullName,
                ClinicName = s.Clinic.Name
            }).ToListAsync();

            var holidays = await _db.HolidayDates
                .Where(h => h.Date >= startDate && h.Date <= endDate)
                .Select(h => new { h.Date, h.Name, h.HolidayType })
                .ToListAsync();

            return Json(new { shifts, holidays });
        }
    }
}
