using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhakhoa.Data;
using Nhakhoa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nhakhoa.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PayrollController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PayrollController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Payroll
        public async Task<IActionResult> Index()
        {
            var config = await GetOrCreateConfig();
            var doctors = await _db.StaffProfiles
                .Include(sp => sp.User)
                .Where(sp => sp.User!.IsActive && sp.User.Role == "Doctor")
                .ToListAsync();

            ViewBag.Config = config;
            ViewBag.Doctors = doctors;
            return View();
        }

        // ─────────────────────────────────────────────────────────────────
        // UC4.1 + UC4.2  — Lấy cấu hình
        // ─────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetConfig()
        {
            var config = await GetOrCreateConfig();
            return Json(config);
        }

        // UC4.1 — Lưu tiền giờ + hệ số học vị
        [HttpPost]
        public async Task<IActionResult> SaveConfig([FromBody] DoctorSalaryConfig input)
        {
            var config = await GetOrCreateConfig();
            config.HourlyRate = input.HourlyRate;
            config.DegreeUniversity = input.DegreeUniversity;
            config.DegreeMaster = input.DegreeMaster;
            config.DegreeDoctorate = input.DegreeDoctorate;
            config.DegreeAssocProf = input.DegreeAssocProf;
            config.DegreeProfessor = input.DegreeProfessor;
            config.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // UC4.2 — Lưu hệ số ngày trong tuần
        [HttpPost]
        public async Task<IActionResult> SaveWeekMultipliers([FromBody] DoctorSalaryConfig input)
        {
            var config = await GetOrCreateConfig();
            config.MultiplierMonday = input.MultiplierMonday;
            config.MultiplierTuesday = input.MultiplierTuesday;
            config.MultiplierWednesday = input.MultiplierWednesday;
            config.MultiplierThursday = input.MultiplierThursday;
            config.MultiplierFriday = input.MultiplierFriday;
            config.MultiplierSaturday = input.MultiplierSaturday;
            config.MultiplierSunday = input.MultiplierSunday;
            config.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // ─────────────────────────────────────────────────────────────────
        // UC4.3 — Hệ số ca phức tạp trong tháng
        // ─────────────────────────────────────────────────────────────────

        // Lấy danh sách ExaminationSession theo bác sĩ + tháng
        [HttpGet]
        public async Task<IActionResult> GetComplexSessions(int doctorId, int month, int year)
        {
            var from = new DateTime(year, month, 1);
            var to = from.AddMonths(1);

            var sessions = await _db.ExaminationSessions
                .Include(e => e.Patient)
                .Where(e => e.DoctorId == doctorId
                         && e.CreatedAt >= from && e.CreatedAt < to)
                .OrderBy(e => e.CreatedAt)
                .Select(e => new
                {
                    e.Id,
                    e.CreatedAt,
                    PatientName = e.Patient != null ? e.Patient.FullName : "—",
                    e.PatientCoefficient,
                    e.IsCompleted,
                    e.Diagnosis
                })
                .ToListAsync();

            return Json(sessions);
        }

        // Cập nhật hệ số phức tạp cho 1 ca khám (legacy - admin trực tiếp gán)
        [HttpPost]
        public async Task<IActionResult> SaveComplexSessionCoefficient([FromBody] UpdateCoeffModel m)
        {
            var session = await _db.ExaminationSessions.FindAsync(m.SessionId);
            if (session == null) return NotFound(new { error = "Không tìm thấy ca khám." });
            if (m.Coefficient < 0 || m.Coefficient > 0.5m)
                return BadRequest(new { error = "Hệ số phải từ 0.0 đến 0.5." });

            session.PatientCoefficient = m.Coefficient;
            await _db.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // ─────────────────────────────────────────────────────────────────
        // UC4.3 — Luồng duyệt ca phức tạp (Admin review)
        // ─────────────────────────────────────────────────────────────────

        // Đếm số ca đang chờ duyệt (dùng cho badge notification)
        [HttpGet]
        public async Task<IActionResult> GetPendingComplexCount()
        {
            var count = await _db.ExaminationSessions
                .CountAsync(e => e.ComplexStatus == "Pending");
            return Json(new { count });
        }

        // Lấy tất cả ca đang chờ duyệt (hoặc theo tháng/bác sĩ)
        [HttpGet]
        public async Task<IActionResult> GetPendingComplexSessions(int? month, int? year)
        {
            var query = _db.ExaminationSessions
                .Include(e => e.Patient)
                .Include(e => e.Doctor)
                    .ThenInclude(d => d!.User)
                .Where(e => e.ComplexStatus == "Pending");

            if (month.HasValue && year.HasValue)
            {
                var from = new DateTime(year.Value, month.Value, 1);
                var to = from.AddMonths(1);
                query = query.Where(e => e.CreatedAt >= from && e.CreatedAt < to);
            }

            var sessions = await query
                .OrderBy(e => e.CreatedAt)
                .Select(e => new
                {
                    e.Id,
                    e.CreatedAt,
                    PatientName = e.Patient != null ? e.Patient.FullName : "—",
                    DoctorName = e.Doctor != null && e.Doctor.User != null ? e.Doctor.User.FullName : "—",
                    DoctorId = e.DoctorId,
                    e.Diagnosis,
                    e.ComplexReason,
                    e.RequestedCoefficient,
                    e.ComplexStatus,
                    e.PatientCoefficient
                })
                .ToListAsync();

            return Json(sessions);
        }

        // Lấy tất cả ca phức tạp (bao gồm đã duyệt/từ chối) — cho bảng lịch sử
        [HttpGet]
        public async Task<IActionResult> GetAllComplexSessions(int? month, int? year, string? status)
        {
            var query = _db.ExaminationSessions
                .Include(e => e.Patient)
                .Include(e => e.Doctor)
                    .ThenInclude(d => d!.User)
                .Where(e => e.ComplexStatus != null);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(e => e.ComplexStatus == status);

            if (month.HasValue && year.HasValue)
            {
                var from = new DateTime(year.Value, month.Value, 1);
                var to = from.AddMonths(1);
                query = query.Where(e => e.CreatedAt >= from && e.CreatedAt < to);
            }

            var sessions = await query
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new
                {
                    e.Id,
                    e.CreatedAt,
                    PatientName = e.Patient != null ? e.Patient.FullName : "—",
                    DoctorName = e.Doctor != null && e.Doctor.User != null ? e.Doctor.User.FullName : "—",
                    DoctorId = e.DoctorId,
                    e.Diagnosis,
                    e.ComplexReason,
                    e.RequestedCoefficient,
                    e.ComplexStatus,
                    e.AdminNote,
                    e.ReviewedAt,
                    e.PatientCoefficient
                })
                .ToListAsync();

            return Json(sessions);
        }

        // Admin duyệt hoặc từ chối ca phức tạp
        [HttpPost]
        public async Task<IActionResult> ReviewComplexSession([FromBody] ReviewComplexModel model)
        {
            var session = await _db.ExaminationSessions.FindAsync(model.SessionId);
            if (session == null) return NotFound(new { error = "Không tìm thấy ca khám." });

            if (session.ComplexStatus != "Pending")
                return BadRequest(new { error = "Ca này không ở trạng thái chờ duyệt." });

            var validActions = new[] { "Approved", "Rejected" };
            if (!validActions.Contains(model.Action))
                return BadRequest(new { error = "Hành động không hợp lệ." });

            if (model.Action == "Approved")
            {
                // Validate hệ số admin muốn duyệt
                if (model.ApprovedCoefficient < 0.1m || model.ApprovedCoefficient > 0.5m)
                    return BadRequest(new { error = "Hệ số duyệt phải từ 0.10 đến 0.50." });

                session.PatientCoefficient = model.ApprovedCoefficient;
                session.ComplexStatus = "Approved";
            }
            else
            {
                // Từ chối — PatientCoefficient giữ nguyên (0.00)
                session.ComplexStatus = "Rejected";
            }

            session.AdminNote = model.AdminNote?.Trim();
            session.ReviewedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            return Ok(new { success = true, newStatus = session.ComplexStatus, patientCoefficient = session.PatientCoefficient });
        }

        public class ReviewComplexModel
        {
            public int SessionId { get; set; }
            public string Action { get; set; } = "Approved"; // "Approved" or "Rejected"
            public decimal ApprovedCoefficient { get; set; } = 0.1m;
            public string? AdminNote { get; set; }
        }

        // ─────────────────────────────────────────────────────────────────
        // UC4.4 — Phiếu lương 1 bác sĩ trong 1 tháng
        // ─────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetDoctorMonthlySlip(int doctorId, int month, int year)
        {
            var config = await GetOrCreateConfig();
            var from = new DateTime(year, month, 1);
            var to = from.AddMonths(1);

            // Thông tin bác sĩ
            var staffProfile = await _db.StaffProfiles
                .Include(sp => sp.User)
                .FirstOrDefaultAsync(sp => sp.Id == doctorId);

            if (staffProfile == null) return NotFound(new { error = "Không tìm thấy bác sĩ." });

            // Hệ số học vị
            decimal degreeCoeff = GetDegreeCoeff(staffProfile.AcademicDegree, config);

            // Lấy ca trực của bác sĩ trong tháng
            var shifts = await _db.Shifts
                .Include(s => s.Clinic)
                .Where(s => s.StaffProfileId == doctorId
                         && s.ShiftDate >= from && s.ShiftDate < to
                         && s.IsActive)
                .ToListAsync();

            // Lấy ShiftSettings để biết số giờ mỗi ca
            var shiftSettings = await _db.ShiftSettings.ToListAsync();

            // Lấy tất cả ExaminationSession của bác sĩ trong tháng để lấy tổng hệ số bệnh nhân
            var sessions = await _db.ExaminationSessions
                .Where(e => e.DoctorId == doctorId && e.CreatedAt >= from && e.CreatedAt < to)
                .ToListAsync();

            // Lấy tất cả đánh giá của bác sĩ trong tháng
            var ratings = await _db.DoctorRatings
                .Where(r => r.DoctorId == doctorId && r.CreatedAt >= from && r.CreatedAt < to)
                .ToListAsync();

            double? averageRating = ratings.Any() ? ratings.Average(r => r.Stars) : null;
            int totalRatings = ratings.Count;

            var shiftDetails = new List<object>();
            decimal totalPay = 0;

            foreach (var shift in shifts)
            {
                // Tìm số giờ ca
                var setting = shiftSettings.FirstOrDefault(ss => ss.ShiftName == shift.ShiftType);
                double durationHours = setting?.DurationHours ?? 0;

                // Hệ số ca theo ngày
                decimal dayMultiplier = GetDayMultiplier(shift.ShiftDate.DayOfWeek, config);

                // Tổng hệ số bệnh nhân trong ngày trực đó
                decimal totalPatientCoeff = sessions
                    .Where(e => e.CreatedAt.Date == shift.ShiftDate.Date)
                    .Sum(e => e.PatientCoefficient);

                // Số giờ quy đổi = Số giờ mỗi ca × (Hệ số ca + Tổng hệ số bệnh nhân)
                decimal convertedHours = (decimal)durationHours * (dayMultiplier + totalPatientCoeff);

                // Tiền 1 ca = Giờ quy đổi × Hệ số bác sĩ × Tiền/giờ
                decimal casePay = convertedHours * degreeCoeff * config.HourlyRate;
                totalPay += casePay;

                shiftDetails.Add(new
                {
                    ShiftId = shift.Id,
                    Date = shift.ShiftDate.ToString("dd/MM/yyyy"),
                    DayName = GetViDayName(shift.ShiftDate.DayOfWeek),
                    ShiftType = shift.ShiftType,
                    ClinicName = shift.Clinic?.Name ?? "—",
                    DurationHours = durationHours,
                    DayMultiplier = dayMultiplier,
                    TotalPatientCoeff = totalPatientCoeff,
                    ConvertedHours = Math.Round(convertedHours, 2),
                    CasePay = Math.Round(casePay, 0)
                });
            }

            return Json(new
            {
                DoctorName = staffProfile.User?.FullName ?? "—",
                StaffCode = staffProfile.StaffCode,
                AcademicDegree = staffProfile.AcademicDegree ?? "Đại học",
                DegreeCoeff = degreeCoeff,
                HourlyRate = config.HourlyRate,
                Month = month,
                Year = year,
                TotalShifts = shifts.Count,
                TotalPay = Math.Round(totalPay, 0),
                AverageRating = averageRating,
                TotalRatings = totalRatings,
                Details = shiftDetails
            });
        }

        // ─────────────────────────────────────────────────────────────────
        // UC4.5 — Báo cáo tất cả bác sĩ trong 1 tháng
        // ─────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAllDoctorsMonthlyReport(int month, int year)
        {
            var config = await GetOrCreateConfig();
            var from = new DateTime(year, month, 1);
            var to = from.AddMonths(1);

            var doctors = await _db.StaffProfiles
                .Include(sp => sp.User)
                .Where(sp => sp.User!.IsActive && sp.User.Role == "Doctor")
                .ToListAsync();

            var shiftSettings = await _db.ShiftSettings.ToListAsync();

            var shifts = await _db.Shifts
                .Where(s => s.ShiftDate >= from && s.ShiftDate < to && s.IsActive)
                .ToListAsync();

            var sessions = await _db.ExaminationSessions
                .Where(e => e.CreatedAt >= from && e.CreatedAt < to)
                .ToListAsync();

            var ratings = await _db.DoctorRatings
                .Where(r => r.CreatedAt >= from && r.CreatedAt < to)
                .ToListAsync();

            var result = new List<object>();

            foreach (var doc in doctors)
            {
                var docShifts = shifts.Where(s => s.StaffProfileId == doc.Id).ToList();
                var docSessions = sessions.Where(e => e.DoctorId == doc.Id).ToList();
                var docRatings = ratings.Where(r => r.DoctorId == doc.Id).ToList();

                decimal degreeCoeff = GetDegreeCoeff(doc.AcademicDegree, config);
                decimal totalPay = 0;
                double totalConvertedHours = 0;

                foreach (var shift in docShifts)
                {
                    var setting = shiftSettings.FirstOrDefault(ss => ss.ShiftName == shift.ShiftType);
                    double dur = setting?.DurationHours ?? 0;
                    decimal dayMul = GetDayMultiplier(shift.ShiftDate.DayOfWeek, config);
                    decimal patCoeff = docSessions.Where(e => e.CreatedAt.Date == shift.ShiftDate.Date).Sum(e => e.PatientCoefficient);
                    decimal convHrs = (decimal)dur * (dayMul + patCoeff);
                    totalConvertedHours += (double)convHrs;
                    totalPay += convHrs * degreeCoeff * config.HourlyRate;
                }

                double? averageRating = docRatings.Any() ? docRatings.Average(r => r.Stars) : null;
                int totalRatings = docRatings.Count;

                result.Add(new
                {
                    DoctorId = doc.Id,
                    DoctorName = doc.User?.FullName ?? "—",
                    AcademicDegree = doc.AcademicDegree ?? "Đại học",
                    DegreeCoeff = degreeCoeff,
                    TotalShifts = docShifts.Count,
                    TotalConvertedHours = Math.Round(totalConvertedHours, 2),
                    TotalPay = Math.Round(totalPay, 0),
                    AverageRating = averageRating,
                    TotalRatings = totalRatings
                });
            }

            return Json(result.OrderByDescending(r => ((dynamic)r).TotalPay));
        }

        // ─────────────────────────────────────────────────────────────────
        // UC4.6 — Lương 1 bác sĩ trong 1 năm (12 tháng) — dùng cho biểu đồ
        // ─────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetDoctorYearlyReport(int doctorId, int year)
        {
            var config = await GetOrCreateConfig();
            var from = new DateTime(year, 1, 1);
            var to = new DateTime(year + 1, 1, 1);

            var staffProfile = await _db.StaffProfiles.Include(sp => sp.User)
                .FirstOrDefaultAsync(sp => sp.Id == doctorId);
            if (staffProfile == null) return NotFound();

            decimal degreeCoeff = GetDegreeCoeff(staffProfile.AcademicDegree, config);

            var shifts = await _db.Shifts
                .Where(s => s.StaffProfileId == doctorId && s.ShiftDate >= from && s.ShiftDate < to && s.IsActive)
                .ToListAsync();

            var sessions = await _db.ExaminationSessions
                .Where(e => e.DoctorId == doctorId && e.CreatedAt >= from && e.CreatedAt < to)
                .ToListAsync();

            var shiftSettings = await _db.ShiftSettings.ToListAsync();

            var monthly = new decimal[12];
            foreach (var shift in shifts)
            {
                int m = shift.ShiftDate.Month - 1;
                var setting = shiftSettings.FirstOrDefault(ss => ss.ShiftName == shift.ShiftType);
                double dur = setting?.DurationHours ?? 0;
                decimal dayMul = GetDayMultiplier(shift.ShiftDate.DayOfWeek, config);
                decimal patCoeff = sessions.Where(e => e.CreatedAt.Date == shift.ShiftDate.Date).Sum(e => e.PatientCoefficient);
                decimal convHrs = (decimal)dur * (dayMul + patCoeff);
                monthly[m] += convHrs * degreeCoeff * config.HourlyRate;
            }

            return Json(new
            {
                DoctorName = staffProfile.User?.FullName ?? "—",
                Year = year,
                Monthly = monthly.Select((v, i) => new { Month = i + 1, Pay = Math.Round(v, 0) })
            });
        }

        // ─────────────────────────────────────────────────────────────────
        // UC4.7 — Tất cả bác sĩ trong 1 năm — dùng cho biểu đồ
        // ─────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAllDoctorsYearlyReport(int year)
        {
            var config = await GetOrCreateConfig();
            var from = new DateTime(year, 1, 1);
            var to = new DateTime(year + 1, 1, 1);

            var doctors = await _db.StaffProfiles.Include(sp => sp.User)
                .Where(sp => sp.User!.IsActive && sp.User.Role == "Doctor").ToListAsync();
            var shifts = await _db.Shifts.Where(s => s.ShiftDate >= from && s.ShiftDate < to && s.IsActive).ToListAsync();
            var sessions = await _db.ExaminationSessions.Where(e => e.CreatedAt >= from && e.CreatedAt < to).ToListAsync();
            var shiftSettings = await _db.ShiftSettings.ToListAsync();
            var ratings = await _db.DoctorRatings.Where(r => r.CreatedAt >= from && r.CreatedAt < to).ToListAsync();

            var result = new List<object>();
            foreach (var doc in doctors)
            {
                var docShifts = shifts.Where(s => s.StaffProfileId == doc.Id).ToList();
                var docSessions = sessions.Where(e => e.DoctorId == doc.Id).ToList();
                var docRatings = ratings.Where(r => r.DoctorId == doc.Id).ToList();

                decimal degreeCoeff = GetDegreeCoeff(doc.AcademicDegree, config);
                var monthly = new decimal[12];
                double totalConvertedHours = 0;

                foreach (var shift in docShifts)
                {
                    int m = shift.ShiftDate.Month - 1;
                    var setting = shiftSettings.FirstOrDefault(ss => ss.ShiftName == shift.ShiftType);
                    double dur = setting?.DurationHours ?? 0;
                    decimal dayMul = GetDayMultiplier(shift.ShiftDate.DayOfWeek, config);
                    decimal patCoeff = docSessions.Where(e => e.CreatedAt.Date == shift.ShiftDate.Date).Sum(e => e.PatientCoefficient);
                    decimal convHrs = (decimal)dur * (dayMul + patCoeff);
                    totalConvertedHours += (double)convHrs;
                    monthly[m] += convHrs * degreeCoeff * config.HourlyRate;
                }

                double? averageRating = docRatings.Any() ? docRatings.Average(r => r.Stars) : null;
                int totalRatings = docRatings.Count;

                result.Add(new
                {
                    DoctorId = doc.Id,
                    DoctorName = doc.User?.FullName ?? "—",
                    AcademicDegree = doc.AcademicDegree ?? "Đại học",
                    DegreeCoeff = degreeCoeff,
                    TotalShifts = docShifts.Count,
                    TotalConvertedHours = Math.Round(totalConvertedHours, 2),
                    Total = Math.Round(monthly.Sum(), 0),
                    AverageRating = averageRating,
                    TotalRatings = totalRatings,
                    Monthly = monthly.Select((v, i) => new { Month = i + 1, Pay = Math.Round(v, 0) })
                });
            }

            return Json(result.OrderByDescending(r => ((dynamic)r).Total));
        }

        // ─────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────
        private async Task<DoctorSalaryConfig> GetOrCreateConfig()
        {
            var config = await _db.DoctorSalaryConfigs.FirstOrDefaultAsync();
            if (config == null)
            {
                config = new DoctorSalaryConfig { Id = 1 };
                _db.DoctorSalaryConfigs.Add(config);
                await _db.SaveChangesAsync();
            }
            return config;
        }

        private static decimal GetDegreeCoeff(string? degree, DoctorSalaryConfig cfg)
        {
            return (degree ?? "").ToLower() switch
            {
                var d when d.Contains("giáo sư") && d.Contains("phó") => cfg.DegreeAssocProf,
                var d when d.Contains("giáo sư") => cfg.DegreeProfessor,
                var d when d.Contains("tiến sĩ") || d.Contains("tiến sỹ") => cfg.DegreeDoctorate,
                var d when d.Contains("thạc sĩ") || d.Contains("thạc sỹ") => cfg.DegreeMaster,
                _ => cfg.DegreeUniversity
            };
        }

        private static decimal GetDayMultiplier(DayOfWeek dow, DoctorSalaryConfig cfg) => dow switch
        {
            DayOfWeek.Monday => cfg.MultiplierMonday,
            DayOfWeek.Tuesday => cfg.MultiplierTuesday,
            DayOfWeek.Wednesday => cfg.MultiplierWednesday,
            DayOfWeek.Thursday => cfg.MultiplierThursday,
            DayOfWeek.Friday => cfg.MultiplierFriday,
            DayOfWeek.Saturday => cfg.MultiplierSaturday,
            DayOfWeek.Sunday => cfg.MultiplierSunday,
            _ => 1m
        };

        private static string GetViDayName(DayOfWeek dow) => dow switch
        {
            DayOfWeek.Monday => "Thứ Hai",
            DayOfWeek.Tuesday => "Thứ Ba",
            DayOfWeek.Wednesday => "Thứ Tư",
            DayOfWeek.Thursday => "Thứ Năm",
            DayOfWeek.Friday => "Thứ Sáu",
            DayOfWeek.Saturday => "Thứ Bảy",
            DayOfWeek.Sunday => "Chủ Nhật",
            _ => "—"
        };

        public class UpdateCoeffModel
        {
            public int SessionId { get; set; }
            public decimal Coefficient { get; set; }
        }
    }
}
