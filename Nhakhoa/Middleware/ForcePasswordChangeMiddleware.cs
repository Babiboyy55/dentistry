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
                var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole == "Patient")
                {
                    await _next(context);
                    return;
                }

                var userIdClaim = context.User.FindFirst("UserId")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var user = await dbContext.Users.FindAsync(userId);
                    if (user != null)
                    {
                        // Chỉ kiểm tra mật khẩu tạm - bỏ check SecurityStamp tạm thời
                        // 1. Bắt buộc đổi mật khẩu nếu đang dùng mật khẩu tạm
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
