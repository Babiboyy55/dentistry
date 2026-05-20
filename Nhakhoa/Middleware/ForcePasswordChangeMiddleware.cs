using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Nhakhoa.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System;

namespace Nhakhoa.Middleware
{
    public class ForcePasswordChangeMiddleware
    {
        private readonly RequestDelegate _next;

        public ForcePasswordChangeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            var path = context.Request.Path.Value ?? "";

            // Bỏ qua tất cả Auth routes và static assets — tránh redirect loop
            var isAuthPath = path.StartsWith("/Auth/", StringComparison.OrdinalIgnoreCase)
                          || path.Equals("/Auth", StringComparison.OrdinalIgnoreCase);
            var isStaticFile = path.Contains("/css/", StringComparison.OrdinalIgnoreCase)
                            || path.Contains("/js/", StringComparison.OrdinalIgnoreCase)
                            || path.Contains("/lib/", StringComparison.OrdinalIgnoreCase)
                            || path.Contains(".css", StringComparison.OrdinalIgnoreCase)
                            || path.Contains(".js", StringComparison.OrdinalIgnoreCase)
                            || path.Contains(".png", StringComparison.OrdinalIgnoreCase)
                            || path.Contains(".ico", StringComparison.OrdinalIgnoreCase);

            if (!isAuthPath && !isStaticFile && context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst("UserId")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var user = await dbContext.Users.FindAsync(userId);
                    if (user != null)
                    {
                        // 1. Force logout nếu SecurityStamp không khớp (tài khoản bị thay đổi/bị khóa)
                        var securityStampClaim = context.User.FindFirst("SecurityStamp")?.Value;
                        System.Console.WriteLine($"[DEBUG] DB Stamp: {user.SecurityStamp}, Claim: {securityStampClaim}"); if (!string.IsNullOrEmpty(securityStampClaim) && securityStampClaim != user.SecurityStamp)
                        {
                            await context.SignOutAsync("Cookies");
                            context.Response.Redirect("/Auth/Login");
                            return;
                        }

                        // 2. Bắt buộc đổi mật khẩu nếu đang dùng mật khẩu tạm
                        if (user.IsTemporaryPassword && !path.Equals("/Auth/ChangePassword", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Response.Redirect("/Auth/ChangePassword");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
