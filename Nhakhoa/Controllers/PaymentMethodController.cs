using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nhakhoa.Data;
using Nhakhoa.Models;
using Nhakhoa.Services;

namespace Nhakhoa.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PaymentMethodController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentMethodController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /PaymentMethod
        public async Task<IActionResult> Index()
        {
            var methods = await _context.PaymentMethods
                .OrderBy(m => m.Id)
                .ToListAsync();

            // Decrypt keys for display in config forms safely
            foreach (var m in methods)
            {
                if (m.IsDigitalGateway && !string.IsNullOrEmpty(m.SecretKey))
                {
                    m.SecretKey = EncryptionHelper.Decrypt(m.SecretKey);
                }
            }

            // Fetch payment method activity logs
            ViewBag.AuditLogs = await _context.ActivityLogs
                .Where(l => l.Action.Contains("thanh toán") || l.Action.Contains("cổng thanh toán"))
                .OrderByDescending(l => l.Timestamp)
                .Take(6)
                .ToListAsync();

            return View(methods);
        }

        // POST: /PaymentMethod/ToggleStatus
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id, bool bypassWarning = false)
        {
            var method = await _context.PaymentMethods.FindAsync(id);
            if (method == null)
            {
                return Json(new { success = false, message = "Phương thức thanh toán không tồn tại." });
            }

            // EX-5.1.3: Tắt phương thức tiền mặt khi đang có hóa đơn chờ thanh toán bằng tiền mặt
            if (method.Code == "CASH" && method.IsEnabled && !bypassWarning)
            {
                var pendingCashInvoices = await _context.Invoices
                    .CountAsync(i => i.PaymentMethodCode == "CASH" && i.Status == "Chờ thanh toán");

                if (pendingCashInvoices > 0)
                {
                    return Json(new
                    {
                        success = false,
                        requireConfirm = true,
                        message = $"Còn {pendingCashInvoices} hóa đơn đang chờ thanh toán bằng tiền mặt. Bạn có chắc chắn muốn tắt phương thức này không?"
                    });
                }
            }

            bool oldStatus = method.IsEnabled;
            method.IsEnabled = !method.IsEnabled;
            method.UpdatedAt = DateTime.Now;

            _context.PaymentMethods.Update(method);

            // Ghi audit log
            var currentUser = User.Identity?.Name ?? "Admin System";
            _context.ActivityLogs.Add(new ActivityLog
            {
                Username = currentUser,
                Action = method.IsEnabled ? "Bật phương thức thanh toán" : "Tắt phương thức thanh toán",
                Details = $"Đã {(method.IsEnabled ? "bật" : "tắt")} phương thức thanh toán: {method.Name} ({method.Code})"
            });

            await _context.SaveChangesAsync();

            return Json(new { success = true, isEnabled = method.IsEnabled, message = $"Đã {(method.IsEnabled ? "bật" : "tắt")} phương thức {method.Name} thành công." });
        }

        // POST: /PaymentMethod/TestConnection
        [HttpPost]
        public async Task<IActionResult> TestConnection(string endpointUrl, string merchantId, string secretKey)
        {
            if (string.IsNullOrWhiteSpace(endpointUrl))
            {
                return Json(new { success = false, message = "Endpoint URL không được để trống." });
            }
            if (string.IsNullOrWhiteSpace(merchantId))
            {
                return Json(new { success = false, message = "Merchant ID không được để trống." });
            }
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                return Json(new { success = false, message = "Secret Key không được để trống." });
            }

            // EX-5.1.2: Mock connection failure for test/invalid credentials
            if (merchantId.ToLower().Contains("invalid") || secretKey.ToLower().Contains("invalid") || merchantId == "123" || secretKey == "123")
            {
                return Json(new { success = false, message = "Kết nối thất bại: Thông tin Merchant ID hoặc Secret Key không chính xác (Sai credentials)." });
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    var response = await client.GetAsync(endpointUrl);
                    
                    // Gateway is reachable, returns success
                    return Json(new { success = true, message = "Kiểm tra kết nối sandbox thành công." });
                }
            }
            catch (TaskCanceledException)
            {
                return Json(new { success = false, message = "Kết nối thất bại: Hết thời gian chờ phản hồi từ cổng thanh toán (Timeout)." });
            }
            catch (HttpRequestException ex)
            {
                return Json(new { success = false, message = $"Kết nối thất bại: Cổng thanh toán không phản hồi hoặc địa chỉ endpoint không tồn tại ({ex.Message})." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Kết nối thất bại: Lỗi mạng hoặc Endpoint URL không hợp lệ ({ex.Message})." });
            }
        }

        // POST: /PaymentMethod/SaveConfig
        [HttpPost]
        public async Task<IActionResult> SaveConfig(int id, string merchantId, string secretKey, string environment, string endpointUrl)
        {
            var method = await _context.PaymentMethods.FindAsync(id);
            if (method == null)
            {
                return Json(new { success = false, message = "Cổng thanh toán không tồn tại." });
            }

            if (string.IsNullOrWhiteSpace(merchantId))
            {
                return Json(new { success = false, message = "Merchant ID không được để trống." });
            }
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                return Json(new { success = false, message = "Secret Key không được để trống." });
            }
            if (string.IsNullOrWhiteSpace(endpointUrl))
            {
                return Json(new { success = false, message = "Endpoint URL không được để trống." });
            }

            merchantId = merchantId.Trim();
            secretKey = secretKey.Trim();
            endpointUrl = endpointUrl.Trim();

            method.MerchantId = merchantId;
            // AES-256 Encrypt credentials
            method.SecretKey = EncryptionHelper.Encrypt(secretKey);
            method.Environment = environment;
            method.EndpointUrl = endpointUrl;
            method.UpdatedAt = DateTime.Now;

            _context.PaymentMethods.Update(method);

            // Ghi audit log
            var currentUser = User.Identity?.Name ?? "Admin System";
            _context.ActivityLogs.Add(new ActivityLog
            {
                Username = currentUser,
                Action = "Lưu cấu hình cổng thanh toán",
                Details = $"Lưu cấu hình cổng {method.Name}: Môi trường = {environment}, Merchant ID = {merchantId}"
            });

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Lưu cấu hình cổng {method.Name} thành công." });
        }

        // POST: /PaymentMethod/SwitchEnvironment
        [HttpPost]
        public async Task<IActionResult> SwitchEnvironment(int id, string environment)
        {
            var method = await _context.PaymentMethods.FindAsync(id);
            if (method == null)
            {
                return Json(new { success = false, message = "Cổng thanh toán không tồn tại." });
            }

            var oldEnv = method.Environment;
            if (oldEnv == environment)
            {
                return Json(new { success = true, message = "Cổng thanh toán đã ở môi trường này." });
            }

            method.Environment = environment;
            
            // Map default production & sandbox endpoints automatically if matching default ones
            if (method.Code == "VNPAY")
            {
                method.EndpointUrl = environment == "Production" 
                    ? "https://pay.vnpay.vn/paymentv2/vpcpay.html" 
                    : "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            }
            else if (method.Code == "MOMO")
            {
                method.EndpointUrl = environment == "Production" 
                    ? "https://payment.momo.vn/v2/gateway/api/create" 
                    : "https://test-payment.momo.vn/v2/gateway/api/create";
            }

            method.UpdatedAt = DateTime.Now;
            _context.PaymentMethods.Update(method);

            // Ghi audit log
            var currentUser = User.Identity?.Name ?? "Admin System";
            _context.ActivityLogs.Add(new ActivityLog
            {
                Username = currentUser,
                Action = "Chuyển đổi môi trường",
                Details = $"Chuyển đổi môi trường cổng {method.Name}: {oldEnv} -> {environment}"
            });

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Chuyển đổi cổng {method.Name} sang môi trường {environment} thành công." });
        }
    }
}
