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
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
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

            int total = await query.CountAsync();
            var patients = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
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
