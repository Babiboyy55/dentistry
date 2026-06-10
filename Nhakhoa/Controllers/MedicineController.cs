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
    [Authorize]
    public class MedicineController : Controller
    {
        private readonly ApplicationDbContext _db;

        public MedicineController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ==========================================
        // GET: /Medicine/Dispense
        // ==========================================
        public async Task<IActionResult> Dispense(string? search, string? dateFilter)
        {
            var query = _db.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                    .ThenInclude(d => d!.User)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Medicine)
                .Where(p => p.Status == "Prescribed")
                .AsQueryable();

            DateTime targetDate = DateTime.Today;
            if (dateFilter == "week")
                query = query.Where(p => p.CreatedAt >= DateTime.Today.AddDays(-7));
            else if (dateFilter == "month")
                query = query.Where(p => p.CreatedAt >= DateTime.Today.AddDays(-30));
            else
                query = query.Where(p => p.CreatedAt.Date == targetDate);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(p =>
                    p.Patient!.FullName.ToLower().Contains(s) ||
                    p.Patient.PatientCode.ToLower().Contains(s) ||
                    (p.Doctor != null && p.Doctor.User != null && p.Doctor.User.FullName.ToLower().Contains(s)));
            }

            var prescriptions = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

            ViewBag.TotalPending = await _db.Prescriptions.CountAsync(p => p.Status == "Prescribed");
            ViewBag.TodayPending = await _db.Prescriptions.CountAsync(p => p.Status == "Prescribed" && p.CreatedAt.Date == DateTime.Today);
            ViewBag.DispensedToday = await _db.Prescriptions.CountAsync(p => p.Status == "Dispensed" && p.CreatedAt.Date == DateTime.Today);
            ViewBag.Search = search;
            ViewBag.DateFilter = dateFilter ?? "today";

            return View(prescriptions);
        }

        // ==========================================
        // GET: /Medicine  (Index — danh sách kho)
        // ==========================================
        public async Task<IActionResult> Index(string? search)
        {
            var query = _db.MedicineInventories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(m => m.MedicineName.ToLower().Contains(s));
            }

            var items = await query.OrderBy(m => m.MedicineName).ToListAsync();

            ViewBag.TotalMedicines = items.Count;
            ViewBag.TotalStock = items.Sum(m => m.StockQuantity);
            ViewBag.LowStockCount = items.Count(m => m.StockQuantity < 10);
            ViewBag.PendingDispenseCount = await _db.Prescriptions.CountAsync(p => p.Status == "Prescribed");
            ViewBag.Search = search;

            return View(items);
        }

        // ==========================================
        // GET: /Medicine/History  (Lịch sử nhập/xuất)
        // ==========================================
        public async Task<IActionResult> History(int? medicineId, string? type, string? dateFrom, string? dateTo, int page = 1)
        {
            int pageSize = 25;

            var query = _db.MedicineTransactions
                .Include(t => t.Medicine)
                .AsQueryable();

            // Filter by medicine
            if (medicineId.HasValue)
                query = query.Where(t => t.MedicineId == medicineId.Value);

            // Filter by type
            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(t => t.TransactionType == type);

            // Filter by date range
            if (DateTime.TryParse(dateFrom, out var from))
                query = query.Where(t => t.CreatedAt.Date >= from.Date);
            if (DateTime.TryParse(dateTo, out var to))
                query = query.Where(t => t.CreatedAt.Date <= to.Date);

            int total = await query.CountAsync();
            var transactions = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Summary stats (all time for selected medicine, or global)
            var statsQuery = _db.MedicineTransactions.AsQueryable();
            if (medicineId.HasValue) statsQuery = statsQuery.Where(t => t.MedicineId == medicineId.Value);

            ViewBag.TotalIn = await statsQuery.Where(t => t.TransactionType == "Nhập kho").SumAsync(t => (int?)t.Quantity) ?? 0;
            ViewBag.TotalOut = await statsQuery.Where(t => t.TransactionType == "Xuất kho").SumAsync(t => (int?)t.Quantity) ?? 0;
            ViewBag.TotalAdjust = await statsQuery.CountAsync(t => t.TransactionType == "Điều chỉnh");
            ViewBag.TodayTransactions = await _db.MedicineTransactions.CountAsync(t => t.CreatedAt.Date == DateTime.Today);

            // Dropdown list of medicines
            ViewBag.Medicines = await _db.MedicineInventories.OrderBy(m => m.MedicineName).ToListAsync();
            ViewBag.SelectedMedicine = medicineId;
            ViewBag.Type = type;
            ViewBag.DateFrom = dateFrom;
            ViewBag.DateTo = dateTo;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.Total = total;

            return View(transactions);
        }

        // ==========================================
        // POST: /Medicine/StockIn  (Nhập kho thêm)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StockIn(int medicineId, int quantity, string? note)
        {
            if (quantity <= 0)
            {
                TempData["ErrorMessage"] = "Số lượng nhập phải lớn hơn 0.";
                return RedirectToAction(nameof(History));
            }

            var medicine = await _db.MedicineInventories.FindAsync(medicineId);
            if (medicine == null) return NotFound();

            int before = medicine.StockQuantity;
            medicine.StockQuantity += quantity;

            var tx = new MedicineTransaction
            {
                MedicineId = medicineId,
                TransactionType = "Nhập kho",
                Quantity = quantity,
                StockBefore = before,
                StockAfter = medicine.StockQuantity,
                Note = string.IsNullOrWhiteSpace(note) ? "Nhập kho thủ công" : note.Trim(),
                CreatedBy = User.Identity?.Name ?? "Unknown",
                CreatedAt = DateTime.Now
            };
            _db.MedicineTransactions.Add(tx);

            var log = new ActivityLog
            {
                Username = User.Identity?.Name ?? "Unknown",
                Action = "Nhập kho thuốc",
                Details = $"Nhập {quantity} {medicine.Unit} '{medicine.MedicineName}'. Tồn: {before} → {medicine.StockQuantity}. Lý do: {tx.Note}",
                Timestamp = DateTime.Now
            };
            _db.ActivityLogs.Add(log);

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã nhập thêm {quantity} {medicine.Unit} '{medicine.MedicineName}' vào kho!";
            return RedirectToAction(nameof(History));
        }

        // ==========================================
        // GET: /Medicine/Create
        // ==========================================
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Medicine/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicineInventory model)
        {
            if (await _db.MedicineInventories.AnyAsync(m => m.MedicineName.ToLower() == model.MedicineName.Trim().ToLower()))
            {
                ModelState.AddModelError("MedicineName", "Tên thuốc này đã tồn tại trong kho.");
            }
            if (model.StockQuantity < 0)
                ModelState.AddModelError("StockQuantity", "Số lượng tồn kho không được âm.");
            if (model.PricePerUnit < 0)
                ModelState.AddModelError("PricePerUnit", "Đơn giá không được âm.");

            if (!ModelState.IsValid)
                return View(model);

            model.MedicineName = model.MedicineName.Trim();
            model.Unit = model.Unit.Trim();

            _db.MedicineInventories.Add(model);
            await _db.SaveChangesAsync();

            // Ghi lịch sử nhập kho lần đầu
            if (model.StockQuantity > 0)
            {
                _db.MedicineTransactions.Add(new MedicineTransaction
                {
                    MedicineId = model.Id,
                    TransactionType = "Nhập kho",
                    Quantity = model.StockQuantity,
                    StockBefore = 0,
                    StockAfter = model.StockQuantity,
                    Note = "Nhập kho lần đầu khi tạo mới thuốc",
                    CreatedBy = User.Identity?.Name ?? "Unknown",
                    CreatedAt = DateTime.Now
                });
            }

            _db.ActivityLogs.Add(new ActivityLog
            {
                Username = User.Identity?.Name ?? "Unknown",
                Action = "Thêm thuốc mới",
                Details = $"Thêm thuốc mới: {model.MedicineName} ({model.StockQuantity} {model.Unit}), đơn giá {model.PricePerUnit:N0} đ",
                Timestamp = DateTime.Now
            });
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã thêm thuốc {model.MedicineName} vào kho thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // GET: /Medicine/Edit/5
        // ==========================================
        public async Task<IActionResult> Edit(int id)
        {
            var medicine = await _db.MedicineInventories.FindAsync(id);
            if (medicine == null) return NotFound();
            return View(medicine);
        }

        // POST: /Medicine/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MedicineInventory model)
        {
            if (id != model.Id) return BadRequest();

            if (await _db.MedicineInventories.AnyAsync(m => m.MedicineName.ToLower() == model.MedicineName.Trim().ToLower() && m.Id != id))
                ModelState.AddModelError("MedicineName", "Tên thuốc này đã tồn tại trong kho.");
            if (model.StockQuantity < 0)
                ModelState.AddModelError("StockQuantity", "Số lượng tồn kho không được âm.");
            if (model.PricePerUnit < 0)
                ModelState.AddModelError("PricePerUnit", "Đơn giá không được âm.");

            if (!ModelState.IsValid)
                return View(model);

            var medicine = await _db.MedicineInventories.FindAsync(id);
            if (medicine == null) return NotFound();

            string oldName = medicine.MedicineName;
            int oldQty = medicine.StockQuantity;
            decimal oldPrice = medicine.PricePerUnit;

            medicine.MedicineName = model.MedicineName.Trim();
            medicine.StockQuantity = model.StockQuantity;
            medicine.PricePerUnit = model.PricePerUnit;
            medicine.Unit = model.Unit.Trim();

            // Ghi lịch sử nếu số lượng thay đổi
            if (model.StockQuantity != oldQty)
            {
                int diff = model.StockQuantity - oldQty;
                _db.MedicineTransactions.Add(new MedicineTransaction
                {
                    MedicineId = id,
                    TransactionType = diff > 0 ? "Nhập kho" : "Điều chỉnh",
                    Quantity = Math.Abs(diff),
                    StockBefore = oldQty,
                    StockAfter = model.StockQuantity,
                    Note = diff > 0
                        ? $"Nhập thêm hàng qua chỉnh sửa kho (tăng {diff})"
                        : $"Điều chỉnh giảm số lượng qua chỉnh sửa ({diff})",
                    CreatedBy = User.Identity?.Name ?? "Unknown",
                    CreatedAt = DateTime.Now
                });
            }

            _db.ActivityLogs.Add(new ActivityLog
            {
                Username = User.Identity?.Name ?? "Unknown",
                Action = "Cập nhật thuốc",
                Details = $"Cập nhật '{oldName}'→'{medicine.MedicineName}', SL {oldQty}→{medicine.StockQuantity}, Giá {oldPrice:N0}→{medicine.PricePerUnit:N0} đ",
                Timestamp = DateTime.Now
            });
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã cập nhật thông tin thuốc {medicine.MedicineName} thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // POST: /Medicine/Delete/5
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var medicine = await _db.MedicineInventories.FindAsync(id);
            if (medicine == null) return NotFound();

            var isReferenced = await _db.PrescriptionItems.AnyAsync(pi => pi.MedicineId == id);
            if (isReferenced)
            {
                TempData["ErrorMessage"] = $"Không thể xóa thuốc {medicine.MedicineName} vì đã có bệnh nhân được kê đơn thuốc này.";
                return RedirectToAction(nameof(Index));
            }

            // Không cho xóa nếu còn lịch sử giao dịch
            var hasTransactions = await _db.MedicineTransactions.AnyAsync(t => t.MedicineId == id);
            if (hasTransactions)
            {
                TempData["ErrorMessage"] = $"Không thể xóa '{medicine.MedicineName}' vì đã có lịch sử giao dịch kho. Hãy đặt số lượng về 0 nếu muốn ngưng sử dụng.";
                return RedirectToAction(nameof(Index));
            }

            _db.MedicineInventories.Remove(medicine);

            _db.ActivityLogs.Add(new ActivityLog
            {
                Username = User.Identity?.Name ?? "Unknown",
                Action = "Xóa thuốc",
                Details = $"Xóa thuốc '{medicine.MedicineName}' (ID: {id}) khỏi kho",
                Timestamp = DateTime.Now
            });
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa thuốc {medicine.MedicineName} khỏi kho thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
