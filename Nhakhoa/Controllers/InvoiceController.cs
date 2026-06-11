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
    [Authorize(Roles = "Admin,Receptionist")]
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _db;

        public InvoiceController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Invoice
        public async Task<IActionResult> Index()
        {
            // 1. Fetch draft invoices that haven't been finalized
            var drafts = await _db.DraftInvoices
                .Include(d => d.Patient)
                .Include(d => d.ExaminationSession)
                    .ThenInclude(es => es!.Doctor)
                        .ThenInclude(doc => doc!.User)
                .Where(d => !d.IsProcessed)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            // 2. Fetch finalized invoices
            var invoices = await _db.Invoices
                .Include(i => i.Patient)
                .Include(i => i.ExaminationSession)
                .OrderByDescending(i => i.IssuedAt)
                .ToListAsync();

            // 3. Fetch refund requests
            var refunds = await _db.Refunds
                .Include(r => r.Invoice)
                    .ThenInclude(i => i!.Patient)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // 4. Fetch daily reconciliations
            var reconciliations = await _db.DailyReconciliations
                .Include(r => r.Details)
                .OrderByDescending(r => r.ReconciliationDate)
                .ToListAsync();

            ViewBag.DraftInvoices = drafts;
            ViewBag.Invoices = invoices;
            ViewBag.Refunds = refunds;
            ViewBag.Reconciliations = reconciliations;

            return View();
        }

        // GET: /Invoice/Create?draftId=...
        public async Task<IActionResult> Create(int draftId)
        {
            var draft = await _db.DraftInvoices
                .Include(d => d.Patient)
                .Include(d => d.ExaminationSession)
                    .ThenInclude(es => es!.Doctor)
                        .ThenInclude(doc => doc!.User)
                .FirstOrDefaultAsync(d => d.Id == draftId);

            if (draft == null || draft.IsProcessed)
            {
                TempData["ErrorMessage"] = "Hóa đơn nháp không tồn tại hoặc đã được xử lý.";
                return RedirectToAction(nameof(Index));
            }

            // Get active payment methods configured by Admin
            var paymentMethods = await _db.PaymentMethods
                .Where(pm => pm.IsEnabled)
                .ToListAsync();

            // Retrieve services from Dental Warranties created in this session (services with warranties)
            var warrantyServices = await _db.DentalWarranties
                .Where(w => w.PatientId == draft.PatientId && w.StartDate.Date == draft.CreatedAt.Date)
                .Include(w => w.MedicalService)
                .Select(w => w.MedicalService)
                .ToListAsync();

            // Get all active medical services so receptionist can adjust or add non-warranted items
            var medicalServices = await _db.MedicalServices
                .Where(ms => ms.IsActive)
                .ToListAsync();

            ViewBag.PaymentMethods = paymentMethods;
            ViewBag.MedicalServices = medicalServices;
            ViewBag.WarrantyServices = warrantyServices;

            return View(draft);
        }

        // POST: /Invoice/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int draftInvoiceId, int patientId, decimal discountAmount, string? notes, string paymentMethodCode, List<int> serviceIds)
        {
            var draft = await _db.DraftInvoices
                .Include(d => d.Patient)
                .FirstOrDefaultAsync(d => d.Id == draftInvoiceId);

            if (draft == null || draft.IsProcessed)
            {
                TempData["ErrorMessage"] = "Hóa đơn nháp không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            if (serviceIds == null || !serviceIds.Any())
            {
                TempData["ErrorMessage"] = "Hóa đơn phải có ít nhất một dịch vụ y tế.";
                return RedirectToAction(nameof(Create), new { draftId = draftInvoiceId });
            }

            // Retrieve services and calculate amounts
            var services = await _db.MedicalServices
                .Where(s => serviceIds.Contains(s.Id))
                .ToListAsync();

            decimal subTotal = services.Sum(s => s.Price);
            decimal vatPercent = 10.00m; // Default VAT is 10%
            decimal vatAmount = Math.Round(subTotal * (vatPercent / 100m), 2);
            decimal totalAmount = subTotal + vatAmount - discountAmount;

            if (totalAmount < 0) totalAmount = 0;

            // Generate unique invoice code: HD-yyyyMMdd-XXXX
            string dateStr = DateTime.Today.ToString("yyyyMMdd");
            int dailyCount = await _db.Invoices.CountAsync(i => i.IssuedAt.Date == DateTime.Today) + 1;
            string invoiceCode = $"HD-{dateStr}-{dailyCount:D4}";

            // Create Invoice
            var invoice = new Invoice
            {
                InvoiceCode = invoiceCode,
                PatientId = patientId > 0 ? patientId : null,
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                VATPercent = vatPercent,
                VATAmount = vatAmount,
                TotalAmount = totalAmount,
                PaymentMethodCode = paymentMethodCode,
                Status = "Chờ thanh toán",
                IssuedAt = DateTime.Now,
                Notes = notes,
                CreatedBy = User.Identity?.Name ?? "Lễ tân",
                ExaminationSessionId = draft.ExaminationSessionId
            };

            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Create Invoice Details
            foreach (var svc in services)
            {
                var detail = new InvoiceDetail
                {
                    InvoiceId = invoice.Id,
                    MedicalServiceId = svc.Id,
                    Quantity = 1,
                    UnitPrice = svc.Price,
                    Amount = svc.Price,
                    Description = svc.Description,
                    CreatedAt = DateTime.Now
                };
                _db.InvoiceDetails.Add(detail);
            }

            // Create a pending Payment
            var payment = new Payment
            {
                InvoiceId = invoice.Id,
                Amount = totalAmount,
                PaymentMethodCode = paymentMethodCode,
                Status = "Chờ thanh toán",
                CreatedAt = DateTime.Now,
                CreatedBy = User.Identity?.Name ?? "Lễ tân"
            };
            _db.Payments.Add(payment);

            // Mark draft invoice as processed
            draft.IsProcessed = true;

            // Audit log
            _db.ActivityLogs.Add(new ActivityLog
            {
                Username = User.Identity?.Name ?? "System",
                Action = "Tạo hóa đơn",
                Details = $"Đã tạo hóa đơn {invoiceCode} cho bệnh nhân {draft.Patient?.FullName ?? "vô danh"}. Trị giá: {totalAmount:N0} VND."
            });

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã tạo hóa đơn {invoiceCode} thành công. Vui lòng tiến hành thanh toán.";
            return RedirectToAction(nameof(Detail), new { id = invoice.Id });
        }

        // GET: /Invoice/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var invoice = await _db.Invoices
                .Include(i => i.Patient)
                .Include(i => i.ExaminationSession)
                    .ThenInclude(es => es!.Doctor)
                        .ThenInclude(d => d!.User)
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.MedicalService)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            // Load active payment methods for checkout dropdown
            ViewBag.PaymentMethods = await _db.PaymentMethods.Where(pm => pm.IsEnabled).ToListAsync();

            return View(invoice);
        }

        // POST: /Invoice/ProcessPayment
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(int invoiceId, string paymentMethodCode, string? gatewayStatus, string? gatewayError)
        {
            var invoice = await _db.Invoices
                .Include(i => i.Patient)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
            {
                return Json(new { success = false, message = "Không tìm thấy hóa đơn." });
            }

            if (invoice.Status == "Đã thanh toán")
            {
                return Json(new { success = true, message = "Hóa đơn đã được thanh toán từ trước." });
            }

            // Get matching pending payment
            var payment = await _db.Payments
                .Where(p => p.InvoiceId == invoiceId && p.Status == "Chờ thanh toán")
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (payment == null)
            {
                payment = new Payment
                {
                    InvoiceId = invoiceId,
                    Amount = invoice.TotalAmount,
                    PaymentMethodCode = paymentMethodCode,
                    Status = "Chờ thanh toán",
                    CreatedAt = DateTime.Now,
                    CreatedBy = User.Identity?.Name ?? "Lễ tân"
                };
                _db.Payments.Add(payment);
            }
            else
            {
                payment.PaymentMethodCode = paymentMethodCode;
                payment.UpdatedAt = DateTime.Now;
            }

            var method = await _db.PaymentMethods.FirstOrDefaultAsync(pm => pm.Code == paymentMethodCode);
            bool isDigital = method?.IsDigitalGateway ?? false;

            if (isDigital)
            {
                // Simulated digital gateway check (VNPay/MoMo) (EX-5.2.2)
                if (gatewayStatus == "success")
                {
                    invoice.Status = "Đã thanh toán";
                    invoice.PaymentMethodCode = paymentMethodCode;
                    invoice.PaidAt = DateTime.Now;

                    payment.Status = "Đã thanh toán";
                    payment.PaidAt = DateTime.Now;
                    payment.TransactionCode = $"GATEWAY-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
                    payment.TransactionReference = $"Ref-{DateTime.Now.Ticks}";

                    _db.ActivityLogs.Add(new ActivityLog
                    {
                        Username = User.Identity?.Name ?? "Gateway",
                        Action = "Thanh toán online thành công",
                        Details = $"Hóa đơn {invoice.InvoiceCode} đã được thanh toán online thành công qua {method?.Name}. Mã giao dịch: {payment.TransactionCode}"
                    });

                    await _db.SaveChangesAsync();
                    return Json(new { success = true, message = "Thanh toán online thành công!" });
                }
                else
                {
                    // Payment failed (EX-5.2.2)
                    payment.Status = "Thất bại";
                    payment.ErrorMessage = gatewayError ?? "Cổng thanh toán từ chối giao dịch hoặc timeout.";
                    
                    _db.ActivityLogs.Add(new ActivityLog
                    {
                        Username = User.Identity?.Name ?? "Gateway",
                        Action = "Thanh toán online thất bại",
                        Details = $"Thử nghiệm thanh toán online cho hóa đơn {invoice.InvoiceCode} thất bại. Lỗi: {payment.ErrorMessage}"
                    });

                    await _db.SaveChangesAsync();
                    return Json(new { success = false, isDigital = true, message = $"Thanh toán online thất bại: {payment.ErrorMessage}" });
                }
            }
            else
            {
                // Physical payment (CASH, BANK) - receptionist handles immediately
                invoice.Status = "Đã thanh toán";
                invoice.PaymentMethodCode = paymentMethodCode;
                invoice.PaidAt = DateTime.Now;

                payment.Status = "Đã thanh toán";
                payment.PaidAt = DateTime.Now;
                payment.TransactionCode = $"CASH-{invoice.Id}";

                _db.ActivityLogs.Add(new ActivityLog
                {
                    Username = User.Identity?.Name ?? "Lễ tân",
                    Action = "Xác nhận thanh toán trực tiếp",
                    Details = $"Đã thu tiền cho hóa đơn {invoice.InvoiceCode} qua phương thức {method?.Name ?? paymentMethodCode}. Số tiền: {invoice.TotalAmount:N0} VND."
                });

                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Thanh toán thành công và ghi nhận doanh thu!" });
            }
        }

        // POST: /Invoice/RequestRefund
        [HttpPost]
        public async Task<IActionResult> RequestRefund(int invoiceId, decimal amount, string reason, string refundMethodCode)
        {
            var invoice = await _db.Invoices.FindAsync(invoiceId);
            if (invoice == null)
            {
                return Json(new { success = false, message = "Không tìm thấy hóa đơn." });
            }

            if (invoice.Status != "Đã thanh toán")
            {
                return Json(new { success = false, message = "Chỉ hóa đơn đã thanh toán mới được phép hoàn tiền." });
            }

            if (amount <= 0 || amount > invoice.TotalAmount)
            {
                return Json(new { success = false, message = "Số tiền hoàn không hợp lệ (Phải từ 0 đến tổng giá trị hóa đơn)." });
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return Json(new { success = false, message = "Vui lòng điền lý do hoàn tiền." });
            }

            // Create refund request
            var refund = new Refund
            {
                InvoiceId = invoiceId,
                Amount = amount,
                Reason = reason.Trim(),
                RefundMethodCode = refundMethodCode,
                Status = "Chờ duyệt",
                CreatedAt = DateTime.Now,
                CreatedBy = User.Identity?.Name ?? "Lễ tân"
            };

            _db.Refunds.Add(refund);
            await _db.SaveChangesAsync();

            // Create Refund Approval log level 1
            var approval = new RefundApproval
            {
                RefundId = refund.Id,
                ApprovalLevel = 1,
                Status = "Chờ duyệt",
                CreatedAt = DateTime.Now
            };
            _db.RefundApprovals.Add(approval);

            // Audit log
            _db.ActivityLogs.Add(new ActivityLog
            {
                Username = User.Identity?.Name ?? "Lễ tân",
                Action = "Tạo yêu cầu hoàn tiền",
                Details = $"Đã tạo yêu cầu hoàn tiền cho hóa đơn {invoice.InvoiceCode}. Số tiền: {amount:N0} VND. Lý do: {reason}"
            });

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Đã gửi yêu cầu hoàn tiền lên Admin chờ phê duyệt." });
        }

        // POST: /Invoice/ApproveRefund
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveRefund(int refundId, string? comment)
        {
            var refund = await _db.Refunds
                .Include(r => r.Invoice)
                .FirstOrDefaultAsync(r => r.Id == refundId);

            if (refund == null)
            {
                return Json(new { success = false, message = "Yêu cầu hoàn tiền không tồn tại." });
            }

            if (refund.Status != "Chờ duyệt")
            {
                return Json(new { success = false, message = "Yêu cầu này đã được xử lý từ trước." });
            }

            var invoice = refund.Invoice;
            if (invoice == null)
            {
                return Json(new { success = false, message = "Không tìm thấy hóa đơn liên quan." });
            }

            // Approve refund
            refund.Status = "Đã duyệt";
            refund.ApprovedAt = DateTime.Now;
            refund.ApprovedBy = User.Identity?.Name ?? "Admin";
            refund.RefundedAt = DateTime.Now;
            refund.RefundBy = User.Identity?.Name ?? "Admin";

            // Update invoice status
            invoice.Status = "Đã hoàn tiền";
            invoice.Notes += $" (Đã hoàn tiền {refund.Amount:N0}đ ngày {DateTime.Now:dd/MM/yyyy})";

            // Record approval history
            var approval = new RefundApproval
            {
                RefundId = refund.Id,
                ApprovalLevel = 1,
                Status = "Đã duyệt",
                Comment = comment ?? "Đã phê duyệt hoàn tiền.",
                ApprovedBy = User.Identity?.Name ?? "Admin",
                CreatedAt = DateTime.Now,
                ApprovedAt = DateTime.Now
            };
            _db.RefundApprovals.Add(approval);

            // Audit log
            _db.ActivityLogs.Add(new ActivityLog
            {
                Username = User.Identity?.Name ?? "Admin",
                Action = "Duyệt yêu cầu hoàn tiền",
                Details = $"Admin đã duyệt hoàn tiền hóa đơn {invoice.InvoiceCode}. Số tiền hoàn: {refund.Amount:N0} VND."
            });

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Đã phê duyệt hoàn tiền thành công!" });
        }

        // POST: /Invoice/RejectRefund
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectRefund(int refundId, string rejectionReason)
        {
            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                return Json(new { success = false, message = "Vui lòng nhập lý do từ chối." });
            }

            var refund = await _db.Refunds
                .Include(r => r.Invoice)
                .FirstOrDefaultAsync(r => r.Id == refundId);

            if (refund == null)
            {
                return Json(new { success = false, message = "Yêu cầu hoàn tiền không tồn tại." });
            }

            if (refund.Status != "Chờ duyệt")
            {
                return Json(new { success = false, message = "Yêu cầu này đã được xử lý từ trước." });
            }

            // Reject refund
            refund.Status = "Từ chối";
            refund.RejectionReason = rejectionReason;
            refund.ApprovedAt = DateTime.Now;
            refund.ApprovedBy = User.Identity?.Name ?? "Admin";

            // Record approval history
            var approval = new RefundApproval
            {
                RefundId = refund.Id,
                ApprovalLevel = 1,
                Status = "Từ chối",
                Comment = rejectionReason,
                ApprovedBy = User.Identity?.Name ?? "Admin",
                CreatedAt = DateTime.Now,
                ApprovedAt = DateTime.Now
            };
            _db.RefundApprovals.Add(approval);

            // Audit log
            _db.ActivityLogs.Add(new ActivityLog
            {
                Username = User.Identity?.Name ?? "Admin",
                Action = "Từ chối hoàn tiền",
                Details = $"Admin đã từ chối yêu cầu hoàn tiền hóa đơn {refund.Invoice?.InvoiceCode}. Lý do: {rejectionReason}"
            });

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Đã từ chối hoàn tiền." });
        }

        // POST: /Invoice/DailyReconcile
        [HttpPost]
        public async Task<IActionResult> DailyReconcile(DateTime date, decimal totalCollectedAmount, string? differenceNotes)
        {
            // Calculate actual total invoice payments of this date
            var invoices = await _db.Invoices
                .Where(i => i.PaidAt.HasValue && i.PaidAt.Value.Date == date.Date && i.Status == "Đã thanh toán")
                .ToListAsync();

            decimal totalExpectedAmount = invoices.Sum(i => i.TotalAmount);
            decimal difference = totalCollectedAmount - totalExpectedAmount;

            // EX-5.2.5 validation: force reasons if there is any mismatch
            if (difference != 0 && string.IsNullOrWhiteSpace(differenceNotes))
            {
                return Json(new { 
                    success = false, 
                    mismatch = true,
                    expectedAmount = totalExpectedAmount,
                    difference = difference,
                    message = $"Số tiền đối soát không khớp (Hệ thống tính: {totalExpectedAmount:N0}đ; Thực tế khai báo: {totalCollectedAmount:N0}đ; Lệch: {difference:N0}đ). Bạn phải nhập lý do chênh lệch chi tiết để ghi nhận kết quả." 
                });
            }

            // Create reconciliation entry
            var reconciliation = new DailyReconciliation
            {
                ReconciliationDate = date.Date,
                Status = difference == 0 ? "Khớp" : "Lệch",
                TotalInvoiceAmount = totalExpectedAmount,
                TotalCollectedAmount = totalCollectedAmount,
                DifferenceAmount = difference,
                DifferenceNotes = differenceNotes,
                CreatedAt = DateTime.Now,
                CreatedBy = User.Identity?.Name ?? "Lễ tân"
            };

            _db.DailyReconciliations.Add(reconciliation);
            await _db.SaveChangesAsync();

            // Group payments by method for reconciliation breakdown
            var payments = await _db.Payments
                .Where(p => p.PaidAt.HasValue && p.PaidAt.Value.Date == date.Date && p.Status == "Đã thanh toán")
                .ToListAsync();

            var grouped = payments.GroupBy(p => p.PaymentMethodCode);

            foreach (var group in grouped)
            {
                var methodCode = group.Key;
                var method = await _db.PaymentMethods.FirstOrDefaultAsync(pm => pm.Code == methodCode);
                var detail = new ReconciliationDetail
                {
                    DailyReconciliationId = reconciliation.Id,
                    PaymentMethodCode = methodCode,
                    PaymentMethodName = method?.Name ?? methodCode,
                    TransactionCount = group.Count(),
                    TotalAmount = group.Sum(p => p.Amount),
                    CreatedAt = DateTime.Now
                };
                _db.ReconciliationDetails.Add(detail);
            }

            // Audit log
            _db.ActivityLogs.Add(new ActivityLog
            {
                Username = User.Identity?.Name ?? "Lễ tân",
                Action = "Đối soát doanh thu",
                Details = $"Đã đối soát doanh thu ngày {date:dd/MM/yyyy}. Trạng thái: {reconciliation.Status}. Lệch: {difference:N0} VND. Ghi chú: {differenceNotes ?? "Không"}"
            });

            await _db.SaveChangesAsync();

            return Json(new { success = true, status = reconciliation.Status, expected = totalExpectedAmount, difference = difference, message = "Đã lưu kết quả đối soát doanh thu ngày hôm nay." });
        }

        // GET: /Invoice/PrintInvoice/5
        public async Task<IActionResult> PrintInvoice(int id)
        {
            var invoice = await _db.Invoices
                .Include(i => i.Patient)
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.MedicalService)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }
    }
}
