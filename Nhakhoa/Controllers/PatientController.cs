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
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PatientController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Patient
        public async Task<IActionResult> Index(string? search, string? gender, string? allergy, string? sortBy, int page = 1)
        {
            // Calculate stats
            ViewBag.StatTotal = await _db.Patients.CountAsync();
            ViewBag.StatToday = await _db.Patients.CountAsync(p => p.CreatedAt >= DateTime.Today);
            ViewBag.StatAllergies = await _db.Patients.CountAsync(p => p.AllergyHistory != null && p.AllergyHistory != "");
            ViewBag.StatAppointments = await _db.Appointments.CountAsync(a => a.AppointmentDate.Month == DateTime.Today.Month && a.AppointmentDate.Year == DateTime.Today.Year);

            int pageSize = 15;
            var query = _db.Patients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(p =>
                    p.FullName.ToLower().Contains(search) ||
                    p.PhoneNumber.Contains(search) ||
                    p.PatientCode.ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(gender))
            {
                query = query.Where(p => p.Gender == gender);
            }

            if (allergy == "true")
            {
                query = query.Where(p => p.AllergyHistory != null && p.AllergyHistory != "");
            }

            if (sortBy == "last_appt")
            {
                query = query.OrderByDescending(p => p.Appointments.Max(a => (DateTime?)a.AppointmentDate) ?? DateTime.MinValue);
            }
            else
            {
                query = query.OrderByDescending(p => p.CreatedAt);
            }

            int total = await query.CountAsync();
            var patients = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Include last appointment info for each patient
            var patientIds = patients.Select(p => p.Id).ToList();
            var lastAppts = await _db.Appointments
                .Where(a => patientIds.Contains(a.PatientId))
                .GroupBy(a => a.PatientId)
                .Select(g => new { PatientId = g.Key, LastDate = g.Max(a => a.AppointmentDate) })
                .ToDictionaryAsync(x => x.PatientId, x => x.LastDate);
            ViewBag.LastAppts = lastAppts;

            ViewBag.Search = search;
            ViewBag.Gender = gender;
            ViewBag.Allergy = allergy;
            ViewBag.SortBy = sortBy;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.Total = total;
            return View(patients);
        }

        // GET: /Patient/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var patient = await _db.Patients
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.StaffProfile)
                        .ThenInclude(sp => sp!.User)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Clinic)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Specialty)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return NotFound();
            return View(patient);
        }

        // GET: /Patient/Create
        public IActionResult Create(string? phone)
        {
            ViewBag.PhoneNumber = phone;
            return View();
        }

        // POST: /Patient/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Patient model)
        {
            // Check duplicate phone
            if (await _db.Patients.AnyAsync(p => p.PhoneNumber == model.PhoneNumber))
            {
                var existing = await _db.Patients.FirstAsync(p => p.PhoneNumber == model.PhoneNumber);
                TempData["Warning"] = $"Số điện thoại đã tồn tại trong hệ thống. Đang chuyển đến hồ sơ: {existing.FullName}";
                return RedirectToAction(nameof(Details), new { id = existing.Id });
            }

            // Generate patient code
            int count = await _db.Patients.CountAsync();
            model.PatientCode = $"BN-{(count + 1):D5}";
            model.CreatedAt = DateTime.Now;
            model.UpdatedAt = DateTime.Now;

            if (!ModelState.IsValid) return View(model);

            _db.Patients.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Đã tạo hồ sơ bệnh nhân {model.FullName} thành công!";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        // GET: /Patient/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var patient = await _db.Patients.FindAsync(id);
            if (patient == null) return NotFound();
            return View(patient);
        }

        // POST: /Patient/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Patient model)
        {
            if (id != model.Id) return BadRequest();

            // Check duplicate phone (excluding self)
            if (await _db.Patients.AnyAsync(p => p.PhoneNumber == model.PhoneNumber && p.Id != id))
            {
                ModelState.AddModelError("PhoneNumber", "Số điện thoại này đã được sử dụng bởi bệnh nhân khác.");
                return View(model);
            }

            if (!ModelState.IsValid) return View(model);

            var patient = await _db.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            patient.FullName = model.FullName;
            patient.PhoneNumber = model.PhoneNumber;
            patient.Email = model.Email;
            patient.DateOfBirth = model.DateOfBirth;
            patient.Gender = model.Gender;
            patient.Address = model.Address;
            patient.AllergyHistory = model.AllergyHistory;
            patient.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật thông tin bệnh nhân!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Patient/Delete/5
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _db.Patients.Include(p => p.Appointments).FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null) return NotFound();

            if (patient.Appointments.Any())
            {
                TempData["Error"] = "Không thể xóa bệnh nhân đã có lịch hẹn trong hệ thống.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _db.Patients.Remove(patient);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa hồ sơ bệnh nhân.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Patient/ClinicalRecord/5
        public async Task<IActionResult> ClinicalRecord(int id)
        {
            var patient = await _db.Patients
                .Include(p => p.PrimaryDoctor)
                    .ThenInclude(d => d!.User)
                .Include(p => p.ToothRecords)
                    .ThenInclude(tr => tr.Doctor)
                        .ThenInclude(d => d!.User)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.StaffProfile)
                        .ThenInclude(sp => sp!.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return NotFound();

            if (User.IsInRole("Receptionist"))
            {
                // Write Audit Log
                var log = new ActivityLog
                {
                    Username = User.Identity?.Name ?? "Unknown",
                    Action = "Truy cập trái phép",
                    Details = $"Lễ tân cố truy cập thông tin lâm sàng của bệnh nhân: {patient.FullName} ({patient.PatientCode})",
                    Timestamp = DateTime.Now
                };
                _db.ActivityLogs.Add(log);
                await _db.SaveChangesAsync();

                return StatusCode(403); // HTTP 403 Forbidden
            }

            var currentUser = await _db.Users
                .Include(u => u.StaffProfile)
                .FirstOrDefaultAsync(u => u.Username == User.Identity.Name);

            // Bác sĩ A cố xem/sửa sơ đồ răng bệnh nhân của Bác sĩ B (EX4.3-01)
            if (User.IsInRole("Doctor") && currentUser?.StaffProfile != null)
            {
                if (patient.PrimaryDoctorId != null && patient.PrimaryDoctorId != currentUser.StaffProfile.Id)
                {
                    // Ghi Audit Log truy cập trái phép
                    var log = new ActivityLog
                    {
                        Username = User.Identity?.Name ?? "Unknown",
                        Action = "Truy cập trái phép",
                        Details = $"Bác sĩ {currentUser.FullName} ({User.Identity?.Name}) cố truy cập bệnh án lâm sàng của bệnh nhân {patient.FullName} ({patient.PatientCode}) thuộc quản lý của Bác sĩ {patient.PrimaryDoctor?.User?.FullName ?? "khác"}.",
                        Timestamp = DateTime.Now
                    };
                    _db.ActivityLogs.Add(log);
                    await _db.SaveChangesAsync();

                    return StatusCode(403); // HTTP 403 Forbidden
                }
            }

            // Lấy danh sách bác sĩ để cho phép gán hoặc chuyển giao bệnh nhân
            ViewBag.Doctors = await _db.StaffProfiles
                .Include(sp => sp.User)
                .Where(sp => sp.User.Role == "Doctor" && sp.User.IsActive)
                .ToListAsync();

            ViewBag.CurrentDoctor = currentUser?.StaffProfile;

            return View(patient);
        }

        // POST: /Patient/AssignPrimaryDoctor
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPrimaryDoctor(int patientId, int doctorId)
        {
            var patient = await _db.Patients.FindAsync(patientId);
            if (patient == null) return NotFound();

            var doctor = await _db.StaffProfiles.Include(sp => sp.User).FirstOrDefaultAsync(sp => sp.Id == doctorId);
            if (doctor == null) return BadRequest("Không tìm thấy bác sĩ.");

            var currentUser = await _db.Users.Include(u => u.StaffProfile).FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
            if (User.IsInRole("Doctor") && currentUser?.StaffProfile?.Id != patient.PrimaryDoctorId && patient.PrimaryDoctorId != null)
            {
                // Bác sĩ thường không được đổi bác sĩ phụ trách của bệnh nhân thuộc người khác
                return Forbid();
            }

            patient.PrimaryDoctorId = doctorId;
            patient.UpdatedAt = DateTime.Now;

            var log = new ActivityLog
            {
                Username = User.Identity?.Name ?? "Unknown",
                Action = "Gán bác sĩ phụ trách chính",
                Details = $"Đã gán Bác sĩ {doctor.User.FullName} làm bác sĩ phụ trách chính cho bệnh nhân {patient.FullName} ({patient.PatientCode}).",
                Timestamp = DateTime.Now
            };
            _db.ActivityLogs.Add(log);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Đã gán bác sĩ phụ trách chính thành công: {doctor.User.FullName}";
            return RedirectToAction(nameof(ClinicalRecord), new { id = patientId });
        }

        // POST: /Patient/SaveToothChart (AJAX)
        [HttpPost]
        public async Task<IActionResult> SaveToothChart([FromBody] ToothChartSaveModel model)
        {
            if (model == null || model.Changes == null || !model.Changes.Any())
            {
                return Json(new { success = false, message = "Không có thay đổi nào cần lưu." });
            }

            var currentUser = await _db.Users.Include(u => u.StaffProfile).FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
            if (currentUser == null)
            {
                return Json(new { success = false, message = "Người dùng không hợp lệ." });
            }

            var patient = await _db.Patients
                .Include(p => p.PrimaryDoctor)
                .FirstOrDefaultAsync(p => p.Id == model.PatientId);

            if (patient == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bệnh nhân." });
            }

            // Phân quyền (EX4.3-01)
            if (User.IsInRole("Doctor") && currentUser.StaffProfile != null)
            {
                if (patient.PrimaryDoctorId != null && patient.PrimaryDoctorId != currentUser.StaffProfile.Id)
                {
                    var logErr = new ActivityLog
                    {
                        Username = User.Identity?.Name ?? "Unknown",
                        Action = "Truy cập trái phép",
                        Details = $"Bác sĩ {currentUser.FullName} cố chỉnh sửa sơ đồ răng bệnh nhân {patient.FullName} ({patient.PatientCode}) của Bác sĩ {patient.PrimaryDoctor?.User?.FullName ?? "khác"}.",
                        Timestamp = DateTime.Now
                    };
                    _db.ActivityLogs.Add(logErr);
                    await _db.SaveChangesAsync();

                    return Json(new { success = false, forbidden = true, message = "Bạn không có quyền chỉnh sửa sơ đồ răng của bệnh nhân này." });
                }
            }

            // Concurrency check (optimistic locking) (EX4.3-02)
            if (patient.ToothChartVersion != model.Version)
            {
                return Json(new { 
                    success = false, 
                    conflict = true, 
                    message = "Xung đột dữ liệu: Sơ đồ răng đã được lưu bởi bác sĩ khác từ trước. Vui lòng tải lại trang để cập nhật thông tin." 
                });
            }

            // Append changes (EX4.3-04)
            foreach (var change in model.Changes)
            {
                var validStatuses = new[] { "Normal", "Caries", "Filling", "RCT", "Extraction", "Implant", "Crown", "Bridge" };
                if (!validStatuses.Contains(change.Status))
                {
                    return Json(new { success = false, message = $"Trạng thái răng không hợp lệ: {change.Status}" });
                }

                var record = new PatientToothRecord
                {
                    PatientId = model.PatientId,
                    ToothNumber = change.ToothNumber,
                    Status = change.Status,
                    Notes = change.Notes,
                    Prescription = change.Prescription,
                    DoctorId = currentUser.StaffProfile?.Id ?? 201, // fallback to first doctor in seed data if Admin
                    AppointmentId = change.AppointmentId,
                    Timestamp = DateTime.Now
                };
                _db.PatientToothRecords.Add(record);
            }

            // Tự động gán làm bác sĩ phụ trách nếu chưa có
            if (patient.PrimaryDoctorId == null && User.IsInRole("Doctor") && currentUser.StaffProfile != null)
            {
                patient.PrimaryDoctorId = currentUser.StaffProfile.Id;
            }

            patient.ToothChartVersion++;
            patient.UpdatedAt = DateTime.Now;

            // Ghi audit log
            var saveLog = new ActivityLog
            {
                Username = User.Identity?.Name ?? "Unknown",
                Action = "Cập nhật sơ đồ răng",
                Details = $"Cập nhật sơ đồ răng cho bệnh nhân {patient.FullName} ({patient.PatientCode}). Số vị trí cập nhật: {model.Changes.Count}. Phiên bản mới: {patient.ToothChartVersion}.",
                Timestamp = DateTime.Now
            };
            _db.ActivityLogs.Add(saveLog);

            await _db.SaveChangesAsync();

            return Json(new { 
                success = true, 
                newVersion = patient.ToothChartVersion, 
                message = "Lưu sơ đồ răng thành công!" 
            });
        }

        // GET: /Patient/GetToothHistory (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetToothHistory(int patientId, int toothNumber)
        {
            var patient = await _db.Patients.FindAsync(patientId);
            if (patient == null) return NotFound("Không tìm thấy bệnh nhân.");

            // Phân quyền (EX4.3-01)
            if (User.IsInRole("Doctor"))
            {
                var currentUser = await _db.Users.Include(u => u.StaffProfile).FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
                if (patient.PrimaryDoctorId != null && patient.PrimaryDoctorId != currentUser?.StaffProfile?.Id)
                {
                    return StatusCode(403);
                }
            }

            var history = await _db.PatientToothRecords
                .Include(tr => tr.Doctor)
                    .ThenInclude(d => d!.User)
                .Where(tr => tr.PatientId == patientId && tr.ToothNumber == toothNumber)
                .OrderByDescending(tr => tr.Timestamp)
                .Select(tr => new
                {
                    tr.Id,
                    tr.ToothNumber,
                    tr.Status,
                    tr.Notes,
                    tr.Prescription,
                    DoctorName = tr.Doctor != null && tr.Doctor.User != null ? tr.Doctor.User.FullName : "Bác sĩ hệ thống",
                    Timestamp = tr.Timestamp.ToString("dd/MM/yyyy HH:mm"),
                    AppointmentId = tr.AppointmentId
                })
                .ToListAsync();

            return Json(history);
        }

        // View Models for saving tooth chart
        public class ToothChartSaveModel
        {
            public int PatientId { get; set; }
            public int Version { get; set; }
            public List<ToothChangeModel> Changes { get; set; } = new();
        }

        public class ToothChangeModel
        {
            public int ToothNumber { get; set; }
            public string Status { get; set; } = "Normal";
            public string? Notes { get; set; }
            public string? Prescription { get; set; }
            public int? AppointmentId { get; set; }
        }


        // GET: /Patient/Search?q=... (AJAX)
        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(new object[] { });

            q = q.Trim().ToLower();
            var results = await _db.Patients
                .Where(p => p.FullName.ToLower().Contains(q) ||
                            p.PhoneNumber.Contains(q) ||
                            p.PatientCode.ToLower().Contains(q))
                .Take(10)
                .Select(p => new { p.Id, p.FullName, p.PhoneNumber, p.PatientCode, p.DateOfBirth })
                .ToListAsync();
            return Json(results);
        }
    }
}
