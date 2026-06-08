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
                .Include(c => c.DentalChairs)
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
                .FirstOrDefaultAsync(c => c.Id == id);

            if (clinic == null)
            {
                return NotFound();
            }

            var today = DateTime.Today;
            var futureShiftsCount = await _context.Shifts
                .CountAsync(s => s.ClinicId == id && s.ShiftDate >= today && s.IsActive);

            var incompleteApptsCount = await _context.Appointments
                .CountAsync(a => (a.ClinicId == id || (a.DentalChair != null && a.DentalChair.ClinicId == id)) && 
                                 a.Status != "Đã khám xong" && a.Status != "Đã hủy");

            return Json(new
            {
                hasConstraints = (futureShiftsCount > 0 || incompleteApptsCount > 0),
                clinicName = clinic.Name,
                futureShifts = futureShiftsCount,
                incompleteAppointments = incompleteApptsCount
            });
        }

        // POST: Clinic/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var clinic = await _context.Clinics
                .FirstOrDefaultAsync(c => c.Id == id);

            if (clinic == null)
            {
                return NotFound();
            }

            var today = DateTime.Today;

            // Check ALT-2: Incomplete appointments
            var incompleteApptsCount = await _context.Appointments
                .CountAsync(a => (a.ClinicId == id || (a.DentalChair != null && a.DentalChair.ClinicId == id)) && 
                                 a.Status != "Đã khám xong" && a.Status != "Đã hủy");

            if (incompleteApptsCount > 0)
            {
                TempData["ErrorMessage"] = $"Phòng khám còn {incompleteApptsCount} lịch hẹn chưa hoàn thành. Vui lòng hủy hoặc chuyển lịch hẹn trước khi xóa.";
                return RedirectToAction(nameof(Index));
            }

            // Check ALT-3: Future shifts
            var futureShiftsCount = await _context.Shifts
                .CountAsync(s => s.ClinicId == id && s.ShiftDate >= today && s.IsActive);

            if (futureShiftsCount > 0)
            {
                TempData["ErrorMessage"] = $"Phòng khám có {futureShiftsCount} ca làm việc trong tương lai. Vui lòng hủy hoặc chuyển ca trước khi xóa.";
                return RedirectToAction(nameof(Index));
            }

            // Nullify references in historical shifts & appointments for this clinic's chairs to avoid Restrict FK errors
            var clinicChairs = await _context.DentalChairs.Where(dc => dc.ClinicId == id).Select(dc => dc.Id).ToListAsync();
            
            var chairShifts = await _context.Shifts.Where(s => s.DentalChairId != null && clinicChairs.Contains(s.DentalChairId.Value)).ToListAsync();
            foreach (var s in chairShifts) s.DentalChairId = null;

            var chairAppts = await _context.Appointments.Where(a => a.DentalChairId != null && clinicChairs.Contains(a.DentalChairId.Value)).ToListAsync();
            foreach (var a in chairAppts) a.DentalChairId = null;

            // Delete clinic (cascades to DentalChairs)
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

        // GET: Clinic/GetChairsJson
        [HttpGet]
        public async Task<IActionResult> GetChairsJson(int clinicId)
        {
            var chairs = await _context.DentalChairs
                .Where(dc => dc.ClinicId == clinicId)
                .OrderBy(dc => dc.ChairCode)
                .ToListAsync();

            var today = DateTime.Today;
            var result = new List<object>();

            foreach (var chair in chairs)
            {
                var futureShifts = await _context.Shifts
                    .CountAsync(s => s.DentalChairId == chair.Id && s.ShiftDate >= today && s.IsActive);

                var incompleteAppts = await _context.Appointments
                    .CountAsync(a => a.DentalChairId == chair.Id && a.Status != "Đã khám xong" && a.Status != "Đã hủy");

                result.Add(new
                {
                    id = chair.Id,
                    clinicId = chair.ClinicId,
                    chairCode = chair.ChairCode,
                    name = chair.Name,
                    status = chair.Status,
                    updatedAt = chair.UpdatedAt.ToString("dd/MM/yyyy HH:mm"),
                    futureShifts = futureShifts,
                    incompleteAppointments = incompleteAppts
                });
            }

            return Json(result);
        }

        // POST: Clinic/SaveChair
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SaveChair(int id, int clinicId, string chairCode, string name, string status, bool bypassWarning = false)
        {
            if (string.IsNullOrWhiteSpace(chairCode))
            {
                return Json(new { success = false, message = "Mã ghế không được để trống." });
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                return Json(new { success = false, message = "Tên ghế không được để trống." });
            }

            chairCode = chairCode.Trim();
            name = name.Trim();

            // Check duplicate code or name in the same clinic (ALT-5)
            if (id == 0) // New
            {
                var duplicateCode = await _context.DentalChairs
                    .AnyAsync(dc => dc.ClinicId == clinicId && dc.ChairCode.ToLower() == chairCode.ToLower());
                var duplicateName = await _context.DentalChairs
                    .AnyAsync(dc => dc.ClinicId == clinicId && dc.Name.ToLower() == name.ToLower());

                if (duplicateCode || duplicateName)
                {
                    return Json(new { success = false, message = "Mã/Tên ghế đã tồn tại trong phòng này." });
                }
            }
            else // Edit
            {
                var duplicateName = await _context.DentalChairs
                    .AnyAsync(dc => dc.ClinicId == clinicId && dc.Id != id && dc.Name.ToLower() == name.ToLower());

                if (duplicateName)
                {
                    return Json(new { success = false, message = "Mã/Tên ghế đã tồn tại trong phòng này." });
                }
            }

            // Check ALT-8: warning when changing status to "Bảo trì" with future appointments
            if (status == "Bảo trì" && id > 0 && !bypassWarning)
            {
                var today = DateTime.Today;
                var futureAppts = await _context.Appointments
                    .CountAsync(a => a.DentalChairId == id && a.Status != "Đã khám xong" && a.Status != "Đã hủy" && a.AppointmentDate.Date >= today);

                if (futureAppts > 0)
                {
                    return Json(new
                    {
                        success = false,
                        requireConfirm = true,
                        message = $"Ghế nha có {futureAppts} lịch hẹn trong tương lai. Bạn có chắc muốn chuyển sang trạng thái Bảo trì không?"
                    });
                }
            }

            var clinic = await _context.Clinics.FindAsync(clinicId);
            if (clinic == null)
            {
                return Json(new { success = false, message = "Phòng khám không tồn tại." });
            }

            var currentUser = User.Identity?.Name ?? "Admin System";
            string details;

            if (id == 0)
            {
                var chair = new DentalChair
                {
                    ClinicId = clinicId,
                    ChairCode = chairCode,
                    Name = name,
                    Status = status,
                    UpdatedAt = DateTime.Now
                };
                _context.DentalChairs.Add(chair);
                details = $"Thêm mới ghế nha: {name} (Mã: {chairCode}, Trạng thái: {status}) thuộc phòng {clinic.Name}";
            }
            else
            {
                var chair = await _context.DentalChairs.FindAsync(id);
                if (chair == null)
                {
                    return NotFound();
                }

                var oldName = chair.Name;
                var oldStatus = chair.Status;

                chair.Name = name;
                chair.Status = status;
                chair.UpdatedAt = DateTime.Now;
                _context.DentalChairs.Update(chair);

                details = $"Cập nhật ghế nha: {name} (Mã: {chair.ChairCode}) thuộc phòng {clinic.Name}.";
                if (oldName != name) details += $" Đổi tên: {oldName} -> {name}.";
                if (oldStatus != status) details += $" Đổi trạng thái: {oldStatus} -> {status}.";
            }

            await _context.SaveChangesAsync();

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Username = currentUser,
                Action = id == 0 ? "Thêm ghế nha" : "Cập nhật ghế nha",
                Details = details
            });
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = id == 0 ? "Thêm ghế nha thành công" : "Cập nhật ghế nha thành công" });
        }

        // POST: Clinic/DeleteChair
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteChair(int id)
        {
            var chair = await _context.DentalChairs
                .Include(dc => dc.Clinic)
                .FirstOrDefaultAsync(dc => dc.Id == id);

            if (chair == null)
            {
                return NotFound();
            }

            var today = DateTime.Today;

            // Check ALT-6: Future shifts
            var futureShifts = await _context.Shifts
                .CountAsync(s => s.DentalChairId == id && s.ShiftDate.Date >= today && s.IsActive);

            if (futureShifts > 0)
            {
                return Json(new { success = false, message = $"Ghế nha có {futureShifts} ca làm việc trong tương lai. Vui lòng hủy phân công trước khi xóa." });
            }

            // Check ALT-7: Incomplete appointments
            var incompleteAppts = await _context.Appointments
                .CountAsync(a => a.DentalChairId == id && a.Status != "Đã khám xong" && a.Status != "Đã hủy");

            if (incompleteAppts > 0)
            {
                return Json(new { success = false, message = $"Ghế nha còn {incompleteAppts} lịch hẹn chưa hoàn thành. Vui lòng hủy hoặc chuyển lịch hẹn trước khi xóa." });
            }

            // Nullify references in historical shifts & appointments for this chair
            var historicalShifts = await _context.Shifts.Where(s => s.DentalChairId == id).ToListAsync();
            foreach (var s in historicalShifts) s.DentalChairId = null;

            var historicalAppts = await _context.Appointments.Where(a => a.DentalChairId == id).ToListAsync();
            foreach (var a in historicalAppts) a.DentalChairId = null;

            _context.DentalChairs.Remove(chair);

            // Log activity
            var currentUser = User.Identity?.Name ?? "Admin System";
            _context.ActivityLogs.Add(new ActivityLog
            {
                Username = currentUser,
                Action = "Xóa ghế nha",
                Details = $"Đã xóa ghế nha: {chair.Name} (Mã: {chair.ChairCode}) thuộc phòng {chair.Clinic?.Name ?? "N/A"}"
            });

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa ghế nha thành công" });
        }
    }
}
