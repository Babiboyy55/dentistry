using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Nhakhoa.Data;
using Nhakhoa.Models;
using Nhakhoa.Hubs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nhakhoa.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHubContext<QueueHub> _hubContext;

        public AppointmentController(ApplicationDbContext db, IHubContext<QueueHub> hubContext)
        {
            _db = db;
            _hubContext = hubContext;
        }

        // GET: /Appointment
        public async Task<IActionResult> Index(string? search, string? status, DateTime? date, int page = 1)
        {
            var statsDate = date ?? DateTime.Today;
            ViewBag.StatConfirmed = await _db.Appointments.CountAsync(a => a.AppointmentDate.Date == statsDate.Date && a.Status == "Đã xác nhận");
            ViewBag.StatInSession = await _db.Appointments.CountAsync(a => a.AppointmentDate.Date == statsDate.Date && a.Status == "Đang khám");
            ViewBag.StatCancelled = await _db.Appointments.CountAsync(a => a.AppointmentDate.Date == statsDate.Date && a.Status == "Đã hủy");
            ViewBag.StatCompleted = await _db.Appointments.CountAsync(a => a.AppointmentDate.Date == statsDate.Date && a.Status == "Đã khám xong");

            int pageSize = 20;
            var query = _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.StaffProfile).ThenInclude(sp => sp!.User)
                .Include(a => a.Clinic)
                .Include(a => a.Specialty)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(a =>
                    a.Patient.FullName.ToLower().Contains(s) ||
                    a.Patient.PhoneNumber.Contains(s) ||
                    a.Patient.PatientCode.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(a => a.Status == status);

            if (date.HasValue)
                query = query.Where(a => a.AppointmentDate.Date == date.Value.Date);
            else
                query = query.Where(a => a.AppointmentDate.Date >= DateTime.Today);

            int total = await query.CountAsync();
            var items = await query
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.TimeSlot)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Date = date?.ToString("yyyy-MM-dd");
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.Total = total;
            return View(items);
        }

        // GET: /Appointment/Create?patientId=5
        public async Task<IActionResult> Create(int? patientId)
        {
            await LoadViewBagDropdowns();
            if (patientId.HasValue)
                ViewBag.SelectedPatient = await _db.Patients.FindAsync(patientId.Value);
            return View();
        }

        private bool SimulateNotificationSend(Appointment model)
        {
            // If notes contains "Lỗi thông báo", trigger simulated failure
            if (!string.IsNullOrEmpty(model.Notes) && model.Notes.Contains("Lỗi thông báo"))
            {
                return false;
            }
            return true;
        }

        // POST: /Appointment/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment model)
        {
            // Validate: not on a holiday
            var holiday = await _db.HolidayDates.FirstOrDefaultAsync(h => h.Date.Date == model.AppointmentDate.Date);
            if (holiday != null)
            {
                ModelState.AddModelError("AppointmentDate", $"Ngày {model.AppointmentDate:dd/MM/yyyy} là ngày nghỉ: {holiday.Name}. Vui lòng chọn ngày khác.");
            }

            // Validate: doctor has shift on that day/session
            var hasShift = await _db.Shifts.AnyAsync(s =>
                s.StaffProfileId == model.StaffProfileId &&
                s.ShiftDate.Date == model.AppointmentDate.Date &&
                s.ShiftType == model.Session &&
                s.IsActive);

            if (!hasShift)
                ModelState.AddModelError("StaffProfileId", "Bác sĩ không có ca trực trong ngày và buổi đã chọn.");

            // Check slot conflict
            var conflict = await _db.Appointments.AnyAsync(a =>
                a.StaffProfileId == model.StaffProfileId &&
                a.AppointmentDate.Date == model.AppointmentDate.Date &&
                a.TimeSlot == model.TimeSlot &&
                a.Status != "Đã hủy");

            if (conflict)
                ModelState.AddModelError("TimeSlot", "Khung giờ này đã có lịch hẹn khác cho bác sĩ trong ngày đã chọn.");

            if (!ModelState.IsValid)
            {
                await LoadViewBagDropdowns();
                ViewBag.SelectedPatient = await _db.Patients.FindAsync(model.PatientId);
                return View(model);
            }

            // Assign queue number
            int queueCount = await _db.Appointments.CountAsync(a =>
                a.StaffProfileId == model.StaffProfileId &&
                a.AppointmentDate.Date == model.AppointmentDate.Date &&
                a.Status != "Đã hủy");
            model.QueueNumber = queueCount + 1;
            model.Status = "Đã xác nhận";
            model.CreatedAt = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.CreatedBy = User.Identity?.Name;
            model.ConcurrencyStamp = Guid.NewGuid();

            try
            {
                _db.Appointments.Add(model);
                await _db.SaveChangesAsync();

                // Audit Log
                var log = new ActivityLog
                {
                    Username = User.Identity?.Name ?? "Unknown",
                    Action = "Đặt lịch hẹn",
                    Details = $"Đặt lịch hẹn mới (ID: {model.Id}) cho bệnh nhân ID {model.PatientId} với bác sĩ ID {model.StaffProfileId} vào {model.AppointmentDate:dd/MM/yyyy} {model.TimeSlot}",
                    Timestamp = DateTime.Now
                };
                _db.ActivityLogs.Add(log);
                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                ModelState.AddModelError("TimeSlot", "Khung giờ này đã được đặt bởi một người dùng khác ngay trước đó. Vui lòng chọn khung giờ khác.");
                await LoadViewBagDropdowns();
                ViewBag.SelectedPatient = await _db.Patients.FindAsync(model.PatientId);
                return View(model);
            }

            // Simulate notifications
            bool notificationSuccess = SimulateNotificationSend(model);
            if (!notificationSuccess)
            {
                var errorLog = new ActivityLog
                {
                    Username = "System",
                    Action = "Lỗi gửi thông báo",
                    Details = $"Không gửi được thông báo SMS/Email tự động cho bệnh nhân ID {model.PatientId} cho lịch hẹn ID {model.Id}",
                    Timestamp = DateTime.Now
                };
                _db.ActivityLogs.Add(errorLog);
                await _db.SaveChangesAsync();

                var patient = await _db.Patients.FindAsync(model.PatientId);
                TempData["Warning"] = $"Đặt lịch thành công! Tuy nhiên, hệ thống không gửi được thông báo tự động (SMS/Email). Vui lòng liên hệ trực tiếp bệnh nhân qua SĐT: {patient?.PhoneNumber}.";
            }
            else
            {
                TempData["Success"] = "Đã đặt lịch hẹn thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Appointment/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var appt = await _db.Appointments.Include(a => a.Patient).FirstOrDefaultAsync(a => a.Id == id);
            if (appt == null) return NotFound();
            if (appt.Status == "Đang khám" || appt.Status == "Đã khám xong")
            {
                TempData["Error"] = "Từ chối dời lịch; bệnh nhân đang trong ca khám hoặc đã khám xong; yêu cầu bác sĩ đóng ca trước.";
                return RedirectToAction(nameof(Index));
            }
            await LoadViewBagDropdowns();
            return View(appt);
        }

        // POST: /Appointment/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment model)
        {
            var appt = await _db.Appointments.FindAsync(id);
            if (appt == null) return NotFound();
            if (appt.Status == "Đang khám" || appt.Status == "Đã khám xong")
            {
                TempData["Error"] = "Từ chối dời lịch; bệnh nhân đang trong ca khám hoặc đã khám xong; yêu cầu bác sĩ đóng ca trước.";
                return RedirectToAction(nameof(Index));
            }

            // Validate holiday
            var holiday = await _db.HolidayDates.FirstOrDefaultAsync(h => h.Date.Date == model.AppointmentDate.Date);
            if (holiday != null)
                ModelState.AddModelError("AppointmentDate", $"Ngày {model.AppointmentDate:dd/MM/yyyy} là ngày nghỉ: {holiday.Name}.");

            // Validate shift
            var hasShift = await _db.Shifts.AnyAsync(s =>
                s.StaffProfileId == model.StaffProfileId &&
                s.ShiftDate.Date == model.AppointmentDate.Date &&
                s.ShiftType == model.Session &&
                s.IsActive);
            if (!hasShift)
                ModelState.AddModelError("StaffProfileId", "Bác sĩ không có ca trực trong ngày và buổi đã chọn.");

            // Slot conflict (excluding self)
            var conflict = await _db.Appointments.AnyAsync(a =>
                a.StaffProfileId == model.StaffProfileId &&
                a.AppointmentDate.Date == model.AppointmentDate.Date &&
                a.TimeSlot == model.TimeSlot &&
                a.Status != "Đã hủy" &&
                a.Id != id);
            if (conflict)
                ModelState.AddModelError("TimeSlot", "Khung giờ này đã có lịch hẹn khác.");

            if (!ModelState.IsValid)
            {
                await LoadViewBagDropdowns();
                return View(model);
            }

            appt.StaffProfileId = model.StaffProfileId;
            appt.ClinicId = model.ClinicId;
            appt.SpecialtyId = model.SpecialtyId;
            appt.AppointmentDate = model.AppointmentDate;
            appt.TimeSlot = model.TimeSlot;
            appt.Session = model.Session;
            appt.Notes = model.Notes;
            appt.UpdatedAt = DateTime.Now;
            appt.ConcurrencyStamp = Guid.NewGuid();

            try
            {
                await _db.SaveChangesAsync();

                // Audit Log
                var log = new ActivityLog
                {
                    Username = User.Identity?.Name ?? "Unknown",
                    Action = "Dời lịch hẹn",
                    Details = $"Dời lịch hẹn (ID: {id}) sang ngày {model.AppointmentDate:dd/MM/yyyy} {model.TimeSlot} với bác sĩ ID {model.StaffProfileId}",
                    Timestamp = DateTime.Now
                };
                _db.ActivityLogs.Add(log);
                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                ModelState.AddModelError("TimeSlot", "Khung giờ này đã được đặt bởi một người dùng khác ngay trước đó. Vui lòng chọn khung giờ khác.");
                await LoadViewBagDropdowns();
                return View(model);
            }

            // Simulate notifications
            bool notificationSuccess = SimulateNotificationSend(model);
            if (!notificationSuccess)
            {
                var errorLog = new ActivityLog
                {
                    Username = "System",
                    Action = "Lỗi gửi thông báo",
                    Details = $"Không gửi được thông báo dời lịch tự động cho bệnh nhân ID {appt.PatientId} cho lịch hẹn ID {id}",
                    Timestamp = DateTime.Now
                };
                _db.ActivityLogs.Add(errorLog);
                await _db.SaveChangesAsync();

                var patient = await _db.Patients.FindAsync(appt.PatientId);
                TempData["Warning"] = $"Dời lịch thành công! Tuy nhiên, hệ thống không gửi được thông báo tự động (SMS/Email). Vui lòng liên hệ trực tiếp bệnh nhân qua SĐT: {patient?.PhoneNumber}.";
            }
            else
            {
                TempData["Success"] = "Đã dời lịch hẹn thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Appointment/Cancel/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var appt = await _db.Appointments.FindAsync(id);
            if (appt == null) return NotFound();

            if (appt.Status == "Đang khám" || appt.Status == "Đã khám xong")
            {
                TempData["Error"] = "Từ chối hủy lịch; bệnh nhân đang trong ca khám hoặc đã khám xong; yêu cầu bác sĩ đóng ca trước.";
                return RedirectToAction(nameof(Index));
            }

            appt.Status = "Đã hủy";
            appt.UpdatedAt = DateTime.Now;
            appt.ConcurrencyStamp = Guid.NewGuid();
            await _db.SaveChangesAsync();

            // Audit Log
            var log = new ActivityLog
            {
                Username = User.Identity?.Name ?? "Unknown",
                Action = "Hủy lịch hẹn",
                Details = $"Hủy lịch hẹn (ID: {id}) của bệnh nhân ID {appt.PatientId}",
                Timestamp = DateTime.Now
            };
            _db.ActivityLogs.Add(log);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Đã hủy lịch hẹn.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Appointment/UpdateStatus (AJAX)
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var appt = await _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.StaffProfile).ThenInclude(sp => sp!.User)
                .Include(a => a.Clinic)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (appt == null) return NotFound();

            // EX-3.3.2: Concurrent calling check
            if (status == "Đang khám" && appt.Status == "Đang khám")
            {
                return BadRequest(new { error = "Bệnh nhân này đã được gọi vào khám bởi một lễ tân khác." });
            }

            string oldStatus = appt.Status;
            appt.Status = status;
            appt.UpdatedAt = DateTime.Now;

            if (status == "Đang khám") appt.CheckedInAt = DateTime.Now;
            if (status == "Đã khám xong") appt.CompletedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            // Activity Log
            var log = new ActivityLog
            {
                Username = User.Identity?.Name ?? "System",
                Action = "Cập nhật hàng chờ",
                Details = $"Cập nhật trạng thái lịch hẹn ID {id} từ '{oldStatus}' thành '{status}'. Bệnh nhân: {appt.Patient?.FullName}.",
                Timestamp = DateTime.Now
            };
            _db.ActivityLogs.Add(log);
            await _db.SaveChangesAsync();

            // Broadcast updates
            await _hubContext.Clients.All.SendAsync("QueueUpdated");

            if (status == "Đang khám")
            {
                var docName = appt.StaffProfile?.User?.FullName ?? "Nha sĩ";
                var clinicName = appt.Clinic?.Name ?? "Phòng khám";
                await _hubContext.Clients.All.SendAsync("PatientCalled", appt.Id, appt.Patient?.FullName, docName, clinicName);
            }

            return Ok(new { success = true, status });
        }

        // GET: /Appointment/Queue?date=...
        public async Task<IActionResult> Queue(DateTime? date)
        {
            var targetDate = date?.Date ?? DateTime.Today;
            var appointments = await _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.StaffProfile).ThenInclude(sp => sp!.User)
                .Include(a => a.Clinic)
                .Where(a => a.AppointmentDate.Date == targetDate && a.Status != "Đã hủy")
                .OrderBy(a => a.IsWalkIn)
                .ThenBy(a => a.TimeSlot)
                .ThenBy(a => a.QueueNumber)
                .ToListAsync();

            ViewBag.TargetDate = targetDate;
            ViewBag.Doctors = await _db.StaffProfiles
                .Include(sp => sp.User)
                .Where(sp => _db.Shifts.Any(s => s.StaffProfileId == sp.Id && s.ShiftDate.Date == targetDate && s.IsActive))
                .ToListAsync();

            return View(appointments);
        }

        // POST: /Appointment/WalkIn (AJAX walk-in registration)
        [HttpPost]
        public async Task<IActionResult> WalkIn(int patientId, int staffProfileId, int? clinicId, string session)
        {
            // EX-3.3.5: Check active doctor shift
            var hasShift = await _db.Shifts.AnyAsync(s =>
                s.StaffProfileId == staffProfileId &&
                s.ShiftDate.Date == DateTime.Today &&
                s.ShiftType == session &&
                s.IsActive);

            if (!hasShift)
                return BadRequest(new { error = "Không có bác sĩ trực ca này. Vui lòng gợi ý bệnh nhân đặt lịch hẹn ngày khác." });

            var patient = await _db.Patients.FindAsync(patientId);
            if (patient == null)
                return BadRequest(new { error = "Không tìm thấy bệnh nhân." });

            int queueCount = await _db.Appointments.CountAsync(a =>
                a.StaffProfileId == staffProfileId &&
                a.AppointmentDate.Date == DateTime.Today &&
                a.Status != "Đã hủy");

            var walkIn = new Appointment
            {
                PatientId = patientId,
                StaffProfileId = staffProfileId,
                ClinicId = clinicId,
                AppointmentDate = DateTime.Today,
                TimeSlot = "Walk-in",
                Session = session,
                Status = "Đang chờ",
                IsWalkIn = true,
                QueueNumber = queueCount + 1,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = User.Identity?.Name,
                ConcurrencyStamp = Guid.NewGuid()
            };

            _db.Appointments.Add(walkIn);
            await _db.SaveChangesAsync();

            // Activity Log
            var log = new ActivityLog
            {
                Username = User.Identity?.Name ?? "System",
                Action = "Tiếp nhận Walk-in",
                Details = $"Tiếp nhận bệnh nhân vãng lai: {patient.FullName} vào ca {session} của bác sĩ ID {staffProfileId}. Số thứ tự: #{walkIn.QueueNumber}.",
                Timestamp = DateTime.Now
            };
            _db.ActivityLogs.Add(log);
            await _db.SaveChangesAsync();

            // Broadcast updates
            await _hubContext.Clients.All.SendAsync("QueueUpdated");

            return Ok(new { success = true, queueNumber = walkIn.QueueNumber });
        }

        // GET available time slots for a doctor/date/session
        [HttpGet]
        public async Task<IActionResult> AvailableSlots(int staffProfileId, string date, string session)
        {
            if (!DateTime.TryParse(date, out var parsedDate))
                return BadRequest();

            var bookedSlots = await _db.Appointments
                .Where(a => a.StaffProfileId == staffProfileId &&
                            a.AppointmentDate.Date == parsedDate.Date &&
                            a.Session == session &&
                            a.Status != "Đã hủy")
                .Select(a => a.TimeSlot)
                .ToListAsync();

            var setting = await _db.ShiftSettings.FirstOrDefaultAsync(s => s.ShiftName == session);
            if (setting == null)
            {
                var fallbackSlots = session == "Sáng"
                    ? new[] { "07:00", "07:30", "08:00", "08:30", "09:00", "09:30", "10:00", "10:30", "11:00", "11:30" }
                    : new[] { "13:00", "13:30", "14:00", "14:30", "15:00", "15:30", "16:00", "16:30" };
                return Json(fallbackSlots.Where(s => !bookedSlots.Contains(s)).ToArray());
            }

            if (!TimeSpan.TryParse(setting.StartTime, out var startTime) || !TimeSpan.TryParse(setting.EndTime, out var endTime))
            {
                return BadRequest();
            }

            var slotsList = new List<string>();
            var current = startTime;
            while (current < endTime)
            {
                slotsList.Add(current.ToString(@"hh\:mm"));
                current = current.Add(TimeSpan.FromMinutes(30));
            }

            var available = slotsList.Where(s => !bookedSlots.Contains(s)).ToArray();
            return Json(available);
        }

        // GET: /Appointment/GetDoctorShifts
        [HttpGet]
        public async Task<IActionResult> GetDoctorShifts(int staffProfileId, string date)
        {
            if (!DateTime.TryParse(date, out var parsedDate))
                return BadRequest();

            var shifts = await _db.Shifts
                .Where(s => s.StaffProfileId == staffProfileId && s.ShiftDate.Date == parsedDate.Date && s.IsActive)
                .Select(s => s.ShiftType)
                .Distinct()
                .ToListAsync();

            return Json(shifts);
        }

        private async Task LoadViewBagDropdowns()
        {
            ViewBag.Doctors = await _db.StaffProfiles
                .Include(sp => sp.User)
                .Where(sp => sp.User!.IsActive && (sp.User.Role == "Doctor"))
                .ToListAsync();
            ViewBag.Clinics = await _db.Clinics.Where(c => c.IsActive).ToListAsync();
            ViewBag.Specialties = await _db.Specialties.ToListAsync();
        }
    }
}
