using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhakhoa.Data;
using Nhakhoa.Models;
using Nhakhoa.ViewModels;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace Nhakhoa.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientPortalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientPortalController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var phone = User.Identity?.Name;
            if (string.IsNullOrEmpty(phone))
            {
                return RedirectToAction("Login", "Auth");
            }

            var account = await _context.PatientAccounts
                .Include(pa => pa.Patient)
                .FirstOrDefaultAsync(pa => pa.PhoneNumber == phone);

            if (account == null)
            {
                await HttpContext.SignOutAsync("Cookies");
                return RedirectToAction("Login", "Auth");
            }

            // Fetch patient appointments if they have a linked patient record
            var appointments = new System.Collections.Generic.List<Appointment>();
            if (account.PatientId.HasValue)
            {
                appointments = await _context.Appointments
                    .Include(a => a.Clinic)
                    .Include(a => a.Specialty)
                    .Include(a => a.StaffProfile)
                    .Where(a => a.PatientId == account.PatientId.Value)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ThenByDescending(a => a.TimeSlot)
                    .ToListAsync();
            }

            ViewBag.Account = account;
            ViewBag.Appointments = appointments;

            return View();
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(PatientChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var phone = User.Identity?.Name;
                if (string.IsNullOrEmpty(phone))
                {
                    return RedirectToAction("Login", "Auth");
                }

                var account = await _context.PatientAccounts.FirstOrDefaultAsync(pa => pa.PhoneNumber == phone);
                if (account != null)
                {
                    if (account.PasswordHash != model.CurrentPassword)
                    {
                        ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                        return View(model);
                    }

                    if (model.NewPassword == model.CurrentPassword)
                    {
                        ModelState.AddModelError("NewPassword", "Mật khẩu mới không được trùng mật khẩu cũ.");
                        return View(model);
                    }

                    account.PasswordHash = model.NewPassword;
                    account.SecurityStamp = Guid.NewGuid().ToString(); // Invalidate old cookies
                    account.UpdatedAt = DateTime.Now;

                    _context.PatientAccounts.Update(account);
                    await _context.SaveChangesAsync();

                    // Log activity
                    var log = new ActivityLog
                    {
                        Username = account.PhoneNumber,
                        Action = "Đổi mật khẩu Patient",
                        Details = $"Bệnh nhân {account.FullName} đã tự đổi mật khẩu thành công."
                    };
                    _context.ActivityLogs.Add(log);
                    await _context.SaveChangesAsync();

                    // Re-sign in the user to refresh security stamp in the cookie
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, account.PhoneNumber),
                        new Claim(ClaimTypes.Role, "Patient"),
                        new Claim("UserId", account.Id.ToString()),
                        new Claim("SecurityStamp", account.SecurityStamp)
                    };

                    var identity = new ClaimsIdentity(claims, "Cookies");
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync("Cookies", principal, new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTime.UtcNow.AddHours(8)
                    });

                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                    return RedirectToAction("Index");
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> HealthProfile()
        {
            var phone = User.Identity?.Name;
            if (string.IsNullOrEmpty(phone))
            {
                return RedirectToAction("Login", "Auth");
            }

            var account = await _context.PatientAccounts
                .Include(pa => pa.Patient)
                .FirstOrDefaultAsync(pa => pa.PhoneNumber == phone);

            if (account == null)
            {
                await HttpContext.SignOutAsync("Cookies");
                return RedirectToAction("Login", "Auth");
            }

            if (!account.PatientId.HasValue)
            {
                ViewBag.Account = account;
                return View("NoProfile"); // Or show an empty state alert inside HealthProfile
            }

            var patient = await _context.Patients
                .Include(p => p.ToothRecords)
                    .ThenInclude(tr => tr.Doctor)
                        .ThenInclude(d => d!.User)
                .Include(p => p.PrimaryDoctor)
                    .ThenInclude(d => d!.User)
                .FirstOrDefaultAsync(p => p.Id == account.PatientId.Value);

            if (patient == null)
            {
                ViewBag.Account = account;
                return View("NoProfile");
            }

            // Fetch completed sessions (EX6.3-02: Project to prevent exposing internal ClinicalNotes)
            var sessions = await _context.ExaminationSessions
                .Include(es => es.Doctor)
                    .ThenInclude(d => d!.User)
                .Where(es => es.PatientId == patient.Id && es.IsCompleted)
                .OrderByDescending(es => es.CreatedAt)
                .ToListAsync();

            var sessionDtos = new System.Collections.Generic.List<PatientExaminationSessionDto>();
            foreach (var sess in sessions)
            {
                // Find matching invoice details
                var svcNames = await _context.Invoices
                    .Where(i => i.ExaminationSessionId == sess.Id)
                    .SelectMany(i => i.InvoiceDetails.Select(d => d.MedicalService!.Name))
                    .ToListAsync();

                if (!svcNames.Any())
                {
                    svcNames = new System.Collections.Generic.List<string> { "Khám tổng quát & Tư vấn" };
                }

                // Find matching prescription items
                var rxItems = await _context.Prescriptions
                    .Where(r => r.ExaminationSessionId == sess.Id)
                    .SelectMany(r => r.Items.Select(item => new PatientPrescriptionItemDto
                    {
                        MedicineName = item.Medicine != null ? item.Medicine.MedicineName : "Thuốc",
                        Dosage = item.Dosage ?? "",
                        Quantity = item.Quantity,
                        Unit = item.Medicine != null ? item.Medicine.Unit : "viên"
                    }))
                    .ToListAsync();

                sessionDtos.Add(new PatientExaminationSessionDto
                {
                    Id = sess.Id,
                    CreatedAt = sess.CreatedAt,
                    Diagnosis = sess.Diagnosis,
                    TreatmentPlanSummary = sess.TreatmentPlanSummary,
                    HomeCareInstructions = sess.HomeCareInstructions,
                    DoctorName = sess.Doctor?.User?.FullName ?? "Bác sĩ hệ thống",
                    PerformedServices = svcNames,
                    PrescriptionItems = rxItems
                });
            }

            // Fetch prescriptions
            var prescriptions = await _context.Prescriptions
                .Include(pr => pr.Doctor)
                    .ThenInclude(d => d!.User)
                .Include(pr => pr.Items)
                    .ThenInclude(i => i.Medicine)
                .Where(pr => pr.PatientId == patient.Id)
                .OrderByDescending(pr => pr.CreatedAt)
                .ToListAsync();

            // Fetch treatment plans
            var treatmentPlans = await _context.TreatmentPlans
                .Include(tp => tp.Doctor)
                    .ThenInclude(d => d!.User)
                .Include(tp => tp.MedicalService)
                .Include(tp => tp.Sessions)
                .Where(tp => tp.PatientId == patient.Id)
                .OrderByDescending(tp => tp.CreatedAt)
                .ToListAsync();

            // Fetch warranties
            var warranties = await _context.DentalWarranties
                .Include(dw => dw.Doctor)
                    .ThenInclude(d => d!.User)
                .Include(dw => dw.MedicalService)
                .Where(dw => dw.PatientId == patient.Id)
                .OrderByDescending(dw => dw.StartDate)
                .ToListAsync();

            // Stats
            int totalVisits = sessions.Count;
            DateTime? lastVisitDate = sessions.FirstOrDefault()?.CreatedAt;
            
            int totalServices = await _context.InvoiceDetails
                .Include(id => id.Invoice)
                .Where(id => id.Invoice!.PatientId == patient.Id)
                .Select(id => id.MedicalServiceId)
                .Distinct()
                .CountAsync();

            if (totalServices == 0)
            {
                totalServices = warranties.Select(w => w.MedicalServiceId).Distinct().Count();
            }

            string lastDoctor = sessions.FirstOrDefault()?.Doctor?.User?.FullName ?? "Bác sĩ hệ thống";

            ViewBag.Account = account;
            ViewBag.ExaminationSessions = sessionDtos;
            ViewBag.Prescriptions = prescriptions;
            ViewBag.TreatmentPlans = treatmentPlans;
            ViewBag.Warranties = warranties;
            ViewBag.TotalVisits = totalVisits;
            ViewBag.LastVisitDate = lastVisitDate;
            ViewBag.TotalServices = totalServices;
            ViewBag.LastDoctor = lastDoctor;

            return View(patient);
        }

        [HttpGet]
        public async Task<IActionResult> GetToothHistory(int toothNumber)
        {
            var phone = User.Identity?.Name;
            if (string.IsNullOrEmpty(phone))
            {
                return Challenge();
            }

            var account = await _context.PatientAccounts.FirstOrDefaultAsync(pa => pa.PhoneNumber == phone);
            if (account == null || !account.PatientId.HasValue)
            {
                return Forbid();
            }

            // EX6.3-01: IDOR check - resolve from the authenticated session
            var patientId = account.PatientId.Value;

            var history = await _context.PatientToothRecords
                .Include(tr => tr.Doctor)
                    .ThenInclude(d => d!.User)
                .Where(tr => tr.PatientId == patientId && tr.ToothNumber == toothNumber)
                .OrderByDescending(tr => tr.Timestamp)
                .Select(tr => new
                {
                    tr.Id,
                    tr.ToothNumber,
                    tr.Status,
                    // tr.Notes is NOT returned to patient portal (EX6.3-02)
                    // tr.Prescription is NOT returned
                    DoctorName = tr.Doctor != null && tr.Doctor.User != null ? tr.Doctor.User.FullName : "Bác sĩ hệ thống",
                    Timestamp = tr.Timestamp.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();

            return Json(history);
        }

        // === UC6.4 — ONLINE BILLING & PAYMENT ===

        [HttpGet]
        public async Task<IActionResult> Billing()
        {
            var phone = User.Identity?.Name;
            if (string.IsNullOrEmpty(phone))
            {
                return RedirectToAction("Login", "Auth");
            }

            var account = await _context.PatientAccounts
                .Include(pa => pa.Patient)
                .FirstOrDefaultAsync(pa => pa.PhoneNumber == phone);

            if (account == null)
            {
                await HttpContext.SignOutAsync("Cookies");
                return RedirectToAction("Login", "Auth");
            }

            if (!account.PatientId.HasValue)
            {
                ViewBag.Account = account;
                return View("NoProfile");
            }

            var invoices = await _context.Invoices
                .Include(i => i.ExaminationSession)
                    .ThenInclude(es => es!.Doctor)
                        .ThenInclude(d => d!.User)
                .Include(i => i.InvoiceDetails)
                .Where(i => i.PatientId == account.PatientId.Value)
                .OrderByDescending(i => i.IssuedAt)
                .ToListAsync();

            decimal totalCost = invoices.Sum(i => i.TotalAmount);
            decimal totalPaid = invoices.Where(i => i.Status == "Đã thanh toán" || i.Status == "Đã hoàn tiền").Sum(i => i.TotalAmount);
            int paidCount = invoices.Count(i => i.Status == "Đã thanh toán");
            int totalCount = invoices.Count;
            int successRate = totalCount > 0 ? (paidCount * 100) / totalCount : 0;

            ViewBag.Account = account;
            ViewBag.TotalCost = totalCost;
            ViewBag.TotalPaid = totalPaid;
            ViewBag.SuccessRate = successRate;
            ViewBag.PendingInvoices = invoices.Count(i => i.Status == "Chờ thanh toán" || i.Status == "Đang xử lý");

            return View(invoices);
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(int id)
        {
            var phone = User.Identity?.Name;
            if (string.IsNullOrEmpty(phone))
            {
                return RedirectToAction("Login", "Auth");
            }

            var account = await _context.PatientAccounts.FirstOrDefaultAsync(pa => pa.PhoneNumber == phone);
            if (account == null || !account.PatientId.HasValue)
            {
                return Forbid();
            }

            var invoice = await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.ExaminationSession)
                    .ThenInclude(es => es!.Doctor)
                        .ThenInclude(d => d!.User)
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.MedicalService)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            // EX6.4-03: IDOR safety check
            if (invoice.PatientId != account.PatientId.Value)
            {
                return Forbid();
            }

            if (invoice.Status == "Đã thanh toán")
            {
                TempData["SuccessMessage"] = "Hóa đơn này đã được thanh toán.";
                return RedirectToAction("Billing");
            }

            // Load active digital gateways (VNPay, MoMo)
            var paymentMethods = await _context.PaymentMethods
                .Where(pm => pm.IsEnabled && pm.IsDigitalGateway)
                .ToListAsync();

            // Default fallbacks in case admin disabled them in settings but patient portal needs to simulate
            if (!paymentMethods.Any())
            {
                paymentMethods.Add(new PaymentMethod { Code = "VNPAY", Name = "Cổng thanh toán VNPay", IsEnabled = true, IsDigitalGateway = true });
                paymentMethods.Add(new PaymentMethod { Code = "MOMO", Name = "Ví điện tử MoMo", IsEnabled = true, IsDigitalGateway = true });
            }

            ViewBag.Account = account;
            ViewBag.PaymentMethods = paymentMethods;

            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessOnlinePayment(int id, string paymentMethodCode)
        {
            var phone = User.Identity?.Name;
            if (string.IsNullOrEmpty(phone))
            {
                return RedirectToAction("Login", "Auth");
            }

            var account = await _context.PatientAccounts.FirstOrDefaultAsync(pa => pa.PhoneNumber == phone);
            if (account == null || !account.PatientId.HasValue)
            {
                return Forbid();
            }

            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == id);
            if (invoice == null)
            {
                return NotFound();
            }

            // EX6.4-03: IDOR safety check
            if (invoice.PatientId != account.PatientId.Value)
            {
                return Forbid();
            }

            if (invoice.Status == "Đã thanh toán")
            {
                return RedirectToAction("PaymentResult", new { id = id, status = "success" });
            }

            // Update status to 'Đang xử lý'
            invoice.Status = "Đang xử lý";
            invoice.PaymentMethodCode = paymentMethodCode;
            _context.Invoices.Update(invoice);
            await _context.SaveChangesAsync();

            // Redirect to Mock Payment Gateway simulator page
            return RedirectToAction("MockPaymentGateway", new { id = id, method = paymentMethodCode });
        }

        [HttpGet]
        public async Task<IActionResult> MockPaymentGateway(int id, string method)
        {
            var phone = User.Identity?.Name;
            if (string.IsNullOrEmpty(phone))
            {
                return RedirectToAction("Login", "Auth");
            }

            var account = await _context.PatientAccounts.FirstOrDefaultAsync(pa => pa.PhoneNumber == phone);
            if (account == null || !account.PatientId.HasValue)
            {
                return Forbid();
            }

            var invoice = await _context.Invoices
                .Include(i => i.Patient)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            // EX6.4-03: IDOR safety check
            if (invoice.PatientId != account.PatientId.Value)
            {
                return Forbid();
            }

            ViewBag.Account = account;
            ViewBag.MethodCode = method;

            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PaymentCallback(int id, string method, string status, string? error)
        {
            var phone = User.Identity?.Name;
            if (string.IsNullOrEmpty(phone))
            {
                return RedirectToAction("Login", "Auth");
            }

            var account = await _context.PatientAccounts.FirstOrDefaultAsync(pa => pa.PhoneNumber == phone);
            if (account == null || !account.PatientId.HasValue)
            {
                return Forbid();
            }

            var invoice = await _context.Invoices
                .Include(i => i.Patient)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            // EX6.4-03: IDOR safety check
            if (invoice.PatientId != account.PatientId.Value)
            {
                return Forbid();
            }

            // EX6.4-04: Idempotency check (prevent double processing)
            if (invoice.Status == "Đã thanh toán")
            {
                return RedirectToAction("PaymentResult", new { id = id, status = "success", alreadyPaid = true });
            }

            // Get or create matching Payment record
            var payment = await _context.Payments
                .Where(p => p.InvoiceId == id && p.Status == "Chờ thanh toán")
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (payment == null)
            {
                payment = new Payment
                {
                    InvoiceId = id,
                    Amount = invoice.TotalAmount,
                    PaymentMethodCode = method,
                    Status = "Chờ thanh toán",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "Patient Portal"
                };
                _context.Payments.Add(payment);
            }

            var methodName = method == "VNPAY" ? "VNPay" : "MoMo";

            if (status == "success")
            {
                invoice.Status = "Đã thanh toán";
                invoice.PaymentMethodCode = method;
                invoice.PaidAt = DateTime.Now;

                payment.Status = "Đã thanh toán";
                payment.PaidAt = DateTime.Now;
                payment.TransactionCode = $"{method.ToUpper()}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
                payment.TransactionReference = $"PortalRef-{DateTime.Now.Ticks}";

                _context.ActivityLogs.Add(new ActivityLog
                {
                    Username = account.PhoneNumber,
                    Action = "Portal thanh toán online thành công",
                    Details = $"Bệnh nhân {account.FullName} đã thanh toán online thành công hóa đơn {invoice.InvoiceCode} qua {methodName}. Số tiền: {invoice.TotalAmount:N0} VND."
                });

                await _context.SaveChangesAsync();
                return RedirectToAction("PaymentResult", new { id = id, status = "success" });
            }
            else
            {
                // EX6.4-01: Revert status to 'Chờ thanh toán' on failure
                invoice.Status = "Chờ thanh toán";

                payment.Status = "Thất bại";
                payment.ErrorMessage = error ?? "Người dùng hủy thanh toán.";

                _context.ActivityLogs.Add(new ActivityLog
                {
                    Username = account.PhoneNumber,
                    Action = "Portal thanh toán online thất bại",
                    Details = $"Bệnh nhân {account.FullName} thanh toán thất bại cho hóa đơn {invoice.InvoiceCode} qua {methodName}. Lỗi: {payment.ErrorMessage}"
                });

                await _context.SaveChangesAsync();
                return RedirectToAction("PaymentResult", new { id = id, status = "failure", error = payment.ErrorMessage });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentResult(int id, string status, string? error, bool alreadyPaid = false)
        {
            var phone = User.Identity?.Name;
            if (string.IsNullOrEmpty(phone))
            {
                return RedirectToAction("Login", "Auth");
            }

            var account = await _context.PatientAccounts.FirstOrDefaultAsync(pa => pa.PhoneNumber == phone);
            if (account == null || !account.PatientId.HasValue)
            {
                return Forbid();
            }

            var invoice = await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            // EX6.4-03: IDOR safety check
            if (invoice.PatientId != account.PatientId.Value)
            {
                return Forbid();
            }

            ViewBag.Account = account;
            ViewBag.Status = status;
            ViewBag.Error = error;
            ViewBag.AlreadyPaid = alreadyPaid;

            return View(invoice);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadInvoicePdf(int id)
        {
            var phone = User.Identity?.Name;
            if (string.IsNullOrEmpty(phone))
            {
                return RedirectToAction("Login", "Auth");
            }

            var account = await _context.PatientAccounts.FirstOrDefaultAsync(pa => pa.PhoneNumber == phone);
            if (account == null || !account.PatientId.HasValue)
            {
                return Forbid();
            }

            var invoice = await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.ExaminationSession)
                    .ThenInclude(es => es!.Doctor)
                        .ThenInclude(d => d!.User)
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.MedicalService)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            // EX6.4-03: IDOR safety check
            if (invoice.PatientId != account.PatientId.Value)
            {
                return Forbid();
            }

            try
            {
                byte[] pdfBytes = GenerateInvoicePdfBytes(invoice);
                return File(pdfBytes, "application/pdf", $"Invoice-{invoice.InvoiceCode}.pdf");
            }
            catch (Exception ex)
            {
                // EX6.4-02: Log error and redirect with message
                var log = new ActivityLog
                {
                    Username = account.PhoneNumber,
                    Action = "Lỗi tải PDF hóa đơn",
                    Details = $"Lỗi khi tạo PDF cho hóa đơn {invoice.InvoiceCode}: {ex.Message}"
                };
                _context.ActivityLogs.Add(log);
                await _context.SaveChangesAsync();

                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo tệp PDF. Vui lòng thử lại sau.";
                return RedirectToAction("Billing");
            }
        }

        private byte[] GenerateInvoicePdfBytes(Invoice invoice)
        {
            // Simple text-based PDF builder
            var sb = new StringBuilder();
            
            // Build the PDF content lines
            var contentLines = new List<string>();
            contentLines.Add("BT");
            contentLines.Add("/F1 18 Tf");
            contentLines.Add("70 730 Td");
            contentLines.Add($"({EscapePdfText("MEDICLINIC DENTAL BILLING")}) Tj");
            
            contentLines.Add("/F1 12 Tf");
            contentLines.Add("0 -40 Td");
            contentLines.Add($"({EscapePdfText("HOA DON THANH TOAN NHA KHOA")}) Tj");
            
            contentLines.Add("0 -30 Td");
            contentLines.Add($"({EscapePdfText($"Ma hoa don: {invoice.InvoiceCode}")}) Tj");
            
            contentLines.Add("0 -20 Td");
            contentLines.Add($"({EscapePdfText($"Ngay phat hanh: {invoice.IssuedAt:dd/MM/yyyy HH:mm}")}) Tj");
            
            contentLines.Add("0 -20 Td");
            contentLines.Add($"({EscapePdfText($"Trang thai: {invoice.Status}")}) Tj");
            
            contentLines.Add("0 -30 Td");
            contentLines.Add($"({EscapePdfText("THONG TIN BENH NHAN:")}) Tj");
            
            contentLines.Add("0 -20 Td");
            contentLines.Add($"({EscapePdfText($"Ten benh nhan: {invoice.Patient?.FullName}")}) Tj");
            
            contentLines.Add("0 -20 Td");
            contentLines.Add($"({EscapePdfText($"So dien thoai: {invoice.Patient?.PhoneNumber}")}) Tj");
            
            contentLines.Add("0 -20 Td");
            contentLines.Add($"({EscapePdfText($"Ma benh nhan: {invoice.Patient?.PatientCode}")}) Tj");
            
            contentLines.Add("0 -30 Td");
            contentLines.Add($"({EscapePdfText("CHI TIET CAC DICH VU DA THUC HIEN:")}) Tj");
            
            foreach (var detail in invoice.InvoiceDetails)
            {
                contentLines.Add("0 -20 Td");
                contentLines.Add($"({EscapePdfText($"- {detail.MedicalService?.Name} x{detail.Quantity}: {detail.Amount:N0} VND")}) Tj");
            }
            
            contentLines.Add("0 -30 Td");
            contentLines.Add($"({EscapePdfText($"Tam tinh: {invoice.SubTotal:N0} VND")}) Tj");
            
            contentLines.Add("0 -20 Td");
            contentLines.Add($"({EscapePdfText($"VAT (10%): {invoice.VATAmount:N0} VND")}) Tj");
            
            contentLines.Add("0 -20 Td");
            contentLines.Add($"({EscapePdfText($"Giam gia: -{invoice.DiscountAmount:N0} VND")}) Tj");
            
            contentLines.Add("0 -25 Td");
            contentLines.Add("/F1 14 Tf");
            contentLines.Add($"({EscapePdfText($"TONG CONG THANH TOAN: {invoice.TotalAmount:N0} VND")}) Tj");
            
            contentLines.Add("ET");
            
            string streamContent = string.Join("\n", contentLines);
            byte[] streamBytes = Encoding.UTF8.GetBytes(streamContent);
            
            // Build the PDF document catalog
            var pdfObjects = new List<string>();
            pdfObjects.Add("%PDF-1.4");
            
            // Object 1: Catalog
            pdfObjects.Add("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj");
            
            // Object 2: Pages
            pdfObjects.Add("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj");
            
            // Object 3: Page
            pdfObjects.Add("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>\nendobj");
            
            // Object 4: Contents Stream
            pdfObjects.Add($"4 0 obj\n<< /Length {streamBytes.Length} >>\nstream\n" + streamContent + "\nendstream\nendobj");
            
            // Object 5: Font (Helvetica)
            pdfObjects.Add("5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj");
            
            // Generate final output
            using (var ms = new MemoryStream())
            {
                using (var writer = new StreamWriter(ms, new UTF8Encoding(false)))
                {
                    foreach (var obj in pdfObjects)
                    {
                        writer.WriteLine(obj);
                    }
                    
                    // Simple PDF trailer
                    writer.WriteLine("xref");
                    writer.WriteLine("0 6");
                    writer.WriteLine("0000000000 65535 f");
                    writer.WriteLine("trailer");
                    writer.WriteLine("<< /Size 6 /Root 1 0 R >>");
                    writer.WriteLine("startxref");
                    writer.WriteLine("10"); // arbitrary startxref offset, modern readers ignore it and scan
                    writer.WriteLine("%%EOF");
                    writer.Flush();
                }
                return ms.ToArray();
            }
        }

        private string EscapePdfText(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            // Replace Vietnamese diacritics to safe ASCII representation for Helvatica
            string result = RemoveSign4VietnameseString(text);
            // Escape parentheses
            result = result.Replace("(", "\\(").Replace(")", "\\)");
            return result;
        }

        private string RemoveSign4VietnameseString(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            string[] SignChars = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };
            for (int i = 1; i < SignChars.Length; i++)
            {
                for (int j = 0; j < SignChars[i].Length; j++)
                {
                    str = str.Replace(SignChars[i][j], SignChars[0][i - 1]);
                }
            }
            return str;
        }
    }
}
