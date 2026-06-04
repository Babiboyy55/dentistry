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

            // Load real EMR details and set into ViewBag
            ViewBag.ExaminationSessions = await _db.ExaminationSessions
                .Include(es => es.Doctor)
                    .ThenInclude(d => d!.User)
                .Where(es => es.PatientId == id)
                .OrderByDescending(es => es.CreatedAt)
                .ToListAsync();

            ViewBag.TreatmentPlans = await _db.TreatmentPlans
                .Include(tp => tp.Doctor)
                    .ThenInclude(d => d!.User)
                .Include(tp => tp.MedicalService)
                .Include(tp => tp.Sessions)
                .Where(tp => tp.PatientId == id)
                .OrderByDescending(tp => tp.CreatedAt)
                .ToListAsync();

            ViewBag.Prescriptions = await _db.Prescriptions
                .Include(p => p.Doctor)
                    .ThenInclude(d => d!.User)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Medicine)
                .Where(p => p.PatientId == id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.Warranties = await _db.DentalWarranties
                .Include(w => w.Doctor)
                    .ThenInclude(d => d!.User)
                .Include(w => w.MedicalService)
                .Where(w => w.PatientId == id)
                .OrderByDescending(w => w.StartDate)
                .ToListAsync();

            ViewBag.MedicalServices = await _db.MedicalServices
                .Where(ms => ms.IsActive)
                .ToListAsync();

            ViewBag.Medicines = await _db.MedicineInventories
                .ToListAsync();

            var activeAppt = patient.Appointments
                .Where(a => a.AppointmentDate.Date == DateTime.Today && (a.Status == "Đang chờ" || a.Status == "Đang khám" || a.Status == "Đã xác nhận"))
                .OrderByDescending(a => a.CheckedInAt)
                .FirstOrDefault();
            ViewBag.ActiveAppointment = activeAppt;

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

        // ==========================================
        // CLINICAL RECORD & EMR SESSION ACTIONS (UC4.1)
        // ==========================================

        // POST: /Patient/SaveEMRSession (AJAX)
        [HttpPost]
        public async Task<IActionResult> SaveEMRSession([FromBody] EMRSaveModel model)
        {
            if (model == null) return BadRequest("Dữ liệu không hợp lệ.");

            var currentUser = await _db.Users.Include(u => u.StaffProfile).FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
            if (currentUser == null) return Json(new { success = false, message = "Người dùng không hợp lệ." });

            var patient = await _db.Patients.FindAsync(model.PatientId);
            if (patient == null) return Json(new { success = false, message = "Không tìm thấy bệnh nhân." });

            // Check permissions (EX4.1-02 / EX4.3-01)
            if (User.IsInRole("Doctor") && currentUser.StaffProfile != null)
            {
                if (patient.PrimaryDoctorId != null && patient.PrimaryDoctorId != currentUser.StaffProfile.Id)
                {
                    var logErr = new ActivityLog
                    {
                        Username = User.Identity?.Name ?? "Unknown",
                        Action = "Truy cập trái phép",
                        Details = $"Bác sĩ {currentUser.FullName} cố chỉnh sửa bệnh án của bệnh nhân {patient.FullName} ({patient.PatientCode}) của bác sĩ khác.",
                        Timestamp = DateTime.Now
                    };
                    _db.ActivityLogs.Add(logErr);
                    await _db.SaveChangesAsync();

                    return Json(new { success = false, forbidden = true, message = "Bạn không có quyền chỉnh sửa bệnh án của bệnh nhân này." });
                }
            }

            ExaminationSession session;
            if (model.Id > 0)
            {
                session = await _db.ExaminationSessions.FindAsync(model.Id);
                if (session == null) return Json(new { success = false, message = "Không tìm thấy phiên khám." });

                // Concurrency Check (EX4.1-01)
                if (session.ConcurrencyStamp != model.ConcurrencyStamp)
                {
                    return Json(new { success = false, conflict = true, message = "Xung đột phiên bản: Bệnh án đã được lưu bởi bác sĩ khác. Vui lòng tải lại trang." });
                }

                session.Diagnosis = model.Diagnosis;
                session.ClinicalNotes = model.ClinicalNotes;
                session.TreatmentPlanSummary = model.TreatmentPlanSummary;
                session.HomeCareInstructions = model.HomeCareInstructions;
                session.PatientCoefficient = model.PatientCoefficient;
                session.ConcurrencyStamp = Guid.NewGuid();
            }
            else
            {
                session = new ExaminationSession
                {
                    PatientId = model.PatientId,
                    DoctorId = currentUser.StaffProfile?.Id ?? 201,
                    AppointmentId = model.AppointmentId,
                    Diagnosis = model.Diagnosis,
                    ClinicalNotes = model.ClinicalNotes,
                    TreatmentPlanSummary = model.TreatmentPlanSummary,
                    HomeCareInstructions = model.HomeCareInstructions,
                    PatientCoefficient = model.PatientCoefficient,
                    IsCompleted = false,
                    CreatedAt = DateTime.Now,
                    ConcurrencyStamp = Guid.NewGuid()
                };
                _db.ExaminationSessions.Add(session);
            }

            // Automatically assign doctor if not assigned
            if (patient.PrimaryDoctorId == null && User.IsInRole("Doctor") && currentUser.StaffProfile != null)
            {
                patient.PrimaryDoctorId = currentUser.StaffProfile.Id;
            }

            await _db.SaveChangesAsync();

            // Log activity
            var auditLog = new ActivityLog
            {
                Username = User.Identity?.Name ?? "Unknown",
                Action = "Lưu bệnh án",
                Details = $"Lưu bệnh án điện tử cho bệnh nhân {patient.FullName} ({patient.PatientCode}).",
                Timestamp = DateTime.Now
            };
            _db.ActivityLogs.Add(auditLog);
            await _db.SaveChangesAsync();

            return Json(new { success = true, sessionId = session.Id, concurrencyStamp = session.ConcurrencyStamp });
        }

        // POST: /Patient/FinishEMRSession (AJAX)
        [HttpPost]
        public async Task<IActionResult> FinishEMRSession([FromBody] EMRFinishModel model)
        {
            if (model == null) return BadRequest("Dữ liệu không hợp lệ.");

            var session = await _db.ExaminationSessions
                .Include(s => s.Patient)
                .FirstOrDefaultAsync(s => s.Id == model.SessionId);
            if (session == null) return Json(new { success = false, message = "Không tìm thấy phiên khám." });

            // Validate patient coefficient (EX4.1-03)
            if (session.PatientCoefficient < 0m || session.PatientCoefficient > 0.5m)
            {
                return Json(new { success = false, message = "Hệ số bệnh nhân bắt buộc phải nằm trong khoảng từ 0.0 đến 0.5." });
            }

            session.IsCompleted = true;

            // Update Appointment Status to "Đã khám xong"
            if (session.AppointmentId.HasValue)
            {
                var appointment = await _db.Appointments.FindAsync(session.AppointmentId.Value);
                if (appointment != null)
                {
                    appointment.Status = "Đã khám xong";
                    appointment.CompletedAt = DateTime.Now;
                }
            }

            // Calculate price based on selected services and tooth chart changes
            decimal totalAmount = 0m;
            if (model.PerformedServiceIds != null && model.PerformedServiceIds.Any())
            {
                var services = await _db.MedicalServices
                    .Where(s => model.PerformedServiceIds.Contains(s.Id))
                    .ToListAsync();
                totalAmount = services.Sum(s => s.Price);
            }

            // Create Draft Invoice
            var draftInvoice = new DraftInvoice
            {
                PatientId = session.PatientId,
                ExaminationSessionId = session.Id,
                TotalAmount = totalAmount,
                CreatedAt = DateTime.Now,
                IsProcessed = false
            };
            _db.DraftInvoices.Add(draftInvoice);

            // Automatically create Dental Warranty if service has warranty
            if (model.PerformedServiceIds != null)
            {
                var warrantableServices = await _db.MedicalServices
                    .Where(s => model.PerformedServiceIds.Contains(s.Id) && s.DefaultWarrantyMonths.HasValue)
                    .ToListAsync();

                foreach (var ws in warrantableServices)
                {
                    var warrantyCode = $"BH-{session.Patient?.PatientCode}-{ws.Id}";
                    var existingWarranty = await _db.DentalWarranties.AnyAsync(w => w.WarrantyCode == warrantyCode);
                    if (!existingWarranty)
                    {
                        var warranty = new DentalWarranty
                        {
                            PatientId = session.PatientId,
                            DoctorId = session.DoctorId,
                            MedicalServiceId = ws.Id,
                            WarrantyCode = warrantyCode,
                            StartDate = DateTime.Now,
                            EndDate = DateTime.Now.AddMonths(ws.DefaultWarrantyMonths!.Value),
                            Terms = $"Bảo hành chính hãng dịch vụ {ws.Name} trong vòng {ws.DefaultWarrantyMonths} tháng theo điều khoản phòng khám.",
                            Status = "Active"
                        };
                        _db.DentalWarranties.Add(warranty);
                    }
                }
            }

            await _db.SaveChangesAsync();

            // Log activity
            var auditLog = new ActivityLog
            {
                Username = User.Identity?.Name ?? "Unknown",
                Action = "Hoàn thành ca khám",
                Details = $"Bác sĩ hoàn thành ca khám cho bệnh nhân {session.Patient?.FullName}. Tạo hóa đơn nháp trị giá {totalAmount:N0} VND.",
                Timestamp = DateTime.Now
            };
            _db.ActivityLogs.Add(auditLog);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Ca khám đã hoàn thành." });
        }


        // ==========================================
        // TREATMENT PLAN ACTIONS (UC4.2)
        // ==========================================

        // POST: /Patient/CreateTreatmentPlan (AJAX)
        [HttpPost]
        public async Task<IActionResult> CreateTreatmentPlan([FromBody] TreatmentPlanCreateModel model)
        {
            if (model == null) return BadRequest();

            var currentUser = await _db.Users.Include(u => u.StaffProfile).FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
            if (currentUser == null) return Json(new { success = false, message = "Người dùng không hợp lệ." });

            var service = await _db.MedicalServices.FindAsync(model.MedicalServiceId);
            if (service == null) return Json(new { success = false, message = "Không tìm thấy dịch vụ." });

            var plan = new TreatmentPlan
            {
                PatientId = model.PatientId,
                DoctorId = currentUser.StaffProfile?.Id ?? 201,
                MedicalServiceId = model.MedicalServiceId,
                Title = model.Title ?? $"Phác đồ điều trị {service.Name}",
                TotalSessions = model.TotalSessions,
                Status = "Active",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            for (int i = 1; i <= model.TotalSessions; i++)
            {
                plan.Sessions.Add(new TreatmentPlanSession
                {
                    SessionNumber = i,
                    Status = "Scheduled",
                    Notes = $"Buổi thứ {i} cho phác đồ {plan.Title}"
                });
            }

            _db.TreatmentPlans.Add(plan);
            await _db.SaveChangesAsync();

            // Audit Log
            var log = new ActivityLog
            {
                Username = User.Identity?.Name ?? "Unknown",
                Action = "Lập phác đồ điều trị",
                Details = $"Lập phác đồ điều trị '{plan.Title}' gồm {plan.TotalSessions} buổi cho bệnh nhân ID: {model.PatientId}",
                Timestamp = DateTime.Now
            };
            _db.ActivityLogs.Add(log);
            await _db.SaveChangesAsync();

            return Json(new { success = true, planId = plan.Id });
        }

        // POST: /Patient/UpdatePlanSession (AJAX)
        [HttpPost]
        public async Task<IActionResult> UpdatePlanSession([FromBody] PlanSessionUpdateModel model)
        {
            var session = await _db.TreatmentPlanSessions
                .Include(s => s.TreatmentPlan)
                .FirstOrDefaultAsync(s => s.Id == model.SessionId);
            if (session == null) return Json(new { success = false, message = "Không tìm thấy buổi khám." });

            session.Status = model.Status;
            session.Notes = model.Notes;
            if (model.Status == "Completed")
            {
                session.CompletedAt = DateTime.Now;
            }

            // Check if all sessions in the plan are completed
            var plan = session.TreatmentPlan;
            if (plan != null)
            {
                var allSessions = await _db.TreatmentPlanSessions
                    .Where(s => s.TreatmentPlanId == plan.Id)
                    .ToListAsync();
                
                var unfinished = allSessions.Any(s => s.Id != session.Id ? s.Status != "Completed" : model.Status != "Completed");
                if (!unfinished)
                {
                    plan.Status = "Completed";
                }
                plan.UpdatedAt = DateTime.Now;
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }

        // POST: /Patient/CancelTreatmentPlan (AJAX)
        [HttpPost]
        public async Task<IActionResult> CancelTreatmentPlan([FromBody] PlanCancelModel model)
        {
            var plan = await _db.TreatmentPlans.FindAsync(model.PlanId);
            if (plan == null) return Json(new { success = false, message = "Không tìm thấy phác đồ." });

            if (string.IsNullOrWhiteSpace(model.Reason))
            {
                return Json(new { success = false, message = "Lý do hủy là bắt buộc." });
            }

            plan.Status = "Cancelled";
            plan.CancellationReason = model.Reason;
            plan.UpdatedAt = DateTime.Now;

            // Mark remaining sessions as Cancelled/Postponed
            var sessions = await _db.TreatmentPlanSessions
                .Where(s => s.TreatmentPlanId == plan.Id && s.Status == "Scheduled")
                .ToListAsync();
            foreach (var s in sessions)
            {
                s.Status = "Postponed";
            }

            // Audit log
            var log = new ActivityLog
            {
                Username = User.Identity?.Name ?? "Unknown",
                Action = "Hủy phác đồ điều trị",
                Details = $"Hủy phác đồ điều trị ID: {plan.Id} vì lý do: {model.Reason}",
                Timestamp = DateTime.Now
            };
            _db.ActivityLogs.Add(log);

            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }


        // ==========================================
        // PRESCRIPTION & DISPENSING ACTIONS (UC4.4)
        // ==========================================

        // GET: /Patient/GetMedicines (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetMedicines()
        {
            var meds = await _db.MedicineInventories
                .Select(m => new { m.Id, m.MedicineName, m.StockQuantity, m.PricePerUnit, m.Unit })
                .ToListAsync();
            return Json(meds);
        }

        // POST: /Patient/SavePrescription (AJAX)
        [HttpPost]
        public async Task<IActionResult> SavePrescription([FromBody] PrescriptionSaveModel model)
        {
            if (model == null || model.Items == null || !model.Items.Any())
                return BadRequest("Đơn thuốc không hợp lệ.");

            var patient = await _db.Patients.FindAsync(model.PatientId);
            if (patient == null) return NotFound("Không tìm thấy bệnh nhân.");

            // Allergy Check (EX4.4-01)
            var medicineIds = model.Items.Select(i => i.MedicineId).ToList();
            var medicines = await _db.MedicineInventories
                .Where(m => medicineIds.Contains(m.Id))
                .ToListAsync();

            bool hasAllergy = false;
            string allergicMeds = "";
            if (!string.IsNullOrEmpty(patient.AllergyHistory))
            {
                var allergyTerms = patient.AllergyHistory.ToLower().Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var med in medicines)
                {
                    if (allergyTerms.Any(term => med.MedicineName.ToLower().Contains(term)))
                    {
                        hasAllergy = true;
                        allergicMeds += (allergicMeds == "" ? "" : ", ") + med.MedicineName;
                    }
                }
            }

            if (hasAllergy && !model.BypassAllergy)
            {
                return Json(new { 
                    success = false, 
                    allergyWarning = true, 
                    message = $"Cảnh báo dị ứng: Bệnh nhân có tiền sử dị ứng liên quan đến thuốc: {allergicMeds}. Bạn có chắc chắn muốn kê đơn thuốc này?" 
                });
            }

            if (hasAllergy && model.BypassAllergy && string.IsNullOrWhiteSpace(model.BypassReason))
            {
                return Json(new { success = false, message = "Bạn phải nhập lý do xác nhận bỏ qua cảnh báo dị ứng." });
            }

            var currentUser = await _db.Users.Include(u => u.StaffProfile).FirstOrDefaultAsync(u => u.Username == User.Identity.Name);

            var prescription = new Prescription
            {
                PatientId = model.PatientId,
                DoctorId = currentUser?.StaffProfile?.Id ?? 201,
                ExaminationSessionId = model.ExaminationSessionId,
                Status = "Prescribed",
                IsAllergyWarningBypassed = hasAllergy,
                AllergyBypassReason = hasAllergy ? model.BypassReason : null,
                CreatedAt = DateTime.Now
            };

            foreach (var item in model.Items)
            {
                prescription.Items.Add(new PrescriptionItem
                {
                    MedicineId = item.MedicineId,
                    Quantity = item.Quantity,
                    Dosage = item.Dosage,
                    DurationDays = item.DurationDays
                });
            }

            _db.Prescriptions.Add(prescription);

            // Audit log if bypassed
            if (hasAllergy)
            {
                var bypassLog = new ActivityLog
                {
                    Username = User.Identity?.Name ?? "Unknown",
                    Action = "Bỏ qua cảnh báo dị ứng",
                    Details = $"Bác sĩ {currentUser?.FullName} bỏ qua cảnh báo dị ứng thuốc '{allergicMeds}' cho bệnh nhân {patient.FullName} với lý do: {model.BypassReason}",
                    Timestamp = DateTime.Now
                };
                _db.ActivityLogs.Add(bypassLog);
            }

            await _db.SaveChangesAsync();

            return Json(new { success = true, prescriptionId = prescription.Id });
        }

        // POST: /Patient/DispensePrescription (AJAX - Dành cho Lễ tân)
        [HttpPost]
        public async Task<IActionResult> DispensePrescription([FromBody] DispenseModel model)
        {
            if (!User.IsInRole("Receptionist") && !User.IsInRole("Admin"))
            {
                return StatusCode(403);
            }

            var prescription = await _db.Prescriptions
                .Include(p => p.Items)
                    .ThenInclude(i => i.Medicine)
                .Include(p => p.Patient)
                .FirstOrDefaultAsync(p => p.Id == model.PrescriptionId);

            if (prescription == null) return Json(new { success = false, message = "Không tìm thấy đơn thuốc." });

            if (prescription.Status == "Dispensed")
            {
                return Json(new { success = false, message = "Đơn thuốc này đã được phát trước đó." });
            }

            // Check stock (EX4.4-02)
            foreach (var item in prescription.Items)
            {
                if (item.Medicine == null || item.Medicine.StockQuantity < item.Quantity)
                {
                    return Json(new { 
                        success = false, 
                        outOfStock = true, 
                        message = $"Hết hàng: Thuốc '{item.Medicine?.MedicineName}' trong kho không đủ (Hiện còn: {item.Medicine?.StockQuantity ?? 0}). Vui lòng báo bác sĩ điều chỉnh đơn thuốc." 
                    });
                }
            }

            // Subtract stock
            foreach (var item in prescription.Items)
            {
                item.Medicine!.StockQuantity -= item.Quantity;
            }

            prescription.Status = "Dispensed";
            await _db.SaveChangesAsync();

            // Audit log
            var log = new ActivityLog
            {
                Username = User.Identity?.Name ?? "Unknown",
                Action = "Phát thuốc",
                Details = $"Phát thuốc cho đơn thuốc ID: {prescription.Id} của bệnh nhân {prescription.Patient?.FullName}.",
                Timestamp = DateTime.Now
            };
            _db.ActivityLogs.Add(log);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Phát thuốc thành công! Đã trừ số lượng tồn kho." });
        }


        // ==========================================
        // DENTAL WARRANTY ACTIONS (UC4.5)
        // ==========================================

        // POST: /Patient/ClaimWarranty (AJAX - Lễ tân tiếp nhận)
        [HttpPost]
        public async Task<IActionResult> ClaimWarranty([FromBody] WarrantyClaimModel model)
        {
            var warranty = await _db.DentalWarranties
                .Include(w => w.Patient)
                .Include(w => w.MedicalService)
                .FirstOrDefaultAsync(w => w.Id == model.WarrantyId);

            if (warranty == null) return Json(new { success = false, message = "Không tìm thấy thông tin bảo hành." });

            // Check if expired
            if (warranty.EndDate < DateTime.Today)
            {
                warranty.Status = "Expired";
                await _db.SaveChangesAsync();
                
                return Json(new { 
                    success = false, 
                    expired = true, 
                    message = $"Bảo hành dịch vụ '{warranty.MedicalService?.Name}' đã hết hạn vào ngày {warranty.EndDate:dd/MM/yyyy}. Chỉ Admin mới có quyền duyệt bảo hành hết hạn." 
                });
            }

            var auditLog = new ActivityLog
            {
                Username = User.Identity?.Name ?? "Unknown",
                Action = "Tiếp nhận bảo hành",
                Details = $"Lễ tân tiếp nhận yêu cầu bảo hành cho bệnh nhân {warranty.Patient?.FullName} ({warranty.WarrantyCode})",
                Timestamp = DateTime.Now
            };
            _db.ActivityLogs.Add(auditLog);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Tiếp nhận yêu cầu bảo hành thành công." });
        }

        // POST: /Patient/OverrideWarranty (AJAX - Chỉ Admin)
        [HttpPost]
        public async Task<IActionResult> OverrideWarranty([FromBody] WarrantyOverrideModel model)
        {
            if (!User.IsInRole("Admin"))
            {
                return StatusCode(403);
            }

            var warranty = await _db.DentalWarranties
                .Include(w => w.Patient)
                .Include(w => w.MedicalService)
                .FirstOrDefaultAsync(w => w.Id == model.WarrantyId);

            if (warranty == null) return Json(new { success = false, message = "Không tìm thấy bảo hành." });

            if (string.IsNullOrWhiteSpace(model.Reason))
            {
                return Json(new { success = false, message = "Lý do ghi đè bảo hành là bắt buộc." });
            }

            warranty.OverrideReason = model.Reason;
            warranty.Status = "Active"; 
            
            if (model.ExtendMonths > 0)
            {
                warranty.EndDate = warranty.EndDate.AddMonths(model.ExtendMonths);
            }

            // Audit log
            var log = new ActivityLog
            {
                Username = User.Identity?.Name ?? "Unknown",
                Action = "Admin ghi đè bảo hành",
                Details = $"Admin ghi đè bảo hành hết hạn ({warranty.WarrantyCode}) của bệnh nhân {warranty.Patient?.FullName} với lý do: {model.Reason}. Gia hạn thêm {model.ExtendMonths} tháng.",
                Timestamp = DateTime.Now
            };
            _db.ActivityLogs.Add(log);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Ghi đè và gia hạn bảo hành thành công." });
        }


        // ==========================================
        // CLINICAL DTOs & VIEW MODELS
        // ==========================================

        public class EMRSaveModel
        {
            public int Id { get; set; }
            public int PatientId { get; set; }
            public int? AppointmentId { get; set; }
            public string Diagnosis { get; set; } = string.Empty;
            public string? ClinicalNotes { get; set; }
            public string? TreatmentPlanSummary { get; set; }
            public string? HomeCareInstructions { get; set; }
            public decimal PatientCoefficient { get; set; }
            public Guid? ConcurrencyStamp { get; set; }
        }

        public class EMRFinishModel
        {
            public int SessionId { get; set; }
            public List<int> PerformedServiceIds { get; set; } = new();
        }

        public class TreatmentPlanCreateModel
        {
            public int PatientId { get; set; }
            public int MedicalServiceId { get; set; }
            public string? Title { get; set; }
            public int TotalSessions { get; set; }
        }

        public class PlanSessionUpdateModel
        {
            public int SessionId { get; set; }
            public string Status { get; set; } = "Completed";
            public string? Notes { get; set; }
        }

        public class PlanCancelModel
        {
            public int PlanId { get; set; }
            public string Reason { get; set; } = string.Empty;
        }

        public class PrescriptionSaveModel
        {
            public int PatientId { get; set; }
            public int? ExaminationSessionId { get; set; }
            public List<PrescriptionItemModel> Items { get; set; } = new();
            public bool BypassAllergy { get; set; }
            public string? BypassReason { get; set; }
        }

        public class PrescriptionItemModel
        {
            public int MedicineId { get; set; }
            public int Quantity { get; set; }
            public string Dosage { get; set; } = string.Empty;
            public int DurationDays { get; set; }
        }

        public class DispenseModel
        {
            public int PrescriptionId { get; set; }
        }

        public class WarrantyClaimModel
        {
            public int WarrantyId { get; set; }
        }

        public class WarrantyOverrideModel
        {
            public int WarrantyId { get; set; }
            public string Reason { get; set; } = string.Empty;
            public int ExtendMonths { get; set; }
        }
    }
}
