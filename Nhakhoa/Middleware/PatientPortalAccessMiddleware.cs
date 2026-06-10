using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Nhakhoa.Data;
using Nhakhoa.Models;
using System.Security.Claims;
using System;

namespace Nhakhoa.Middleware
{
    public class PatientPortalAccessMiddleware
    {
        private readonly RequestDelegate _next;

        public PatientPortalAccessMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            var path = context.Request.Path.Value ?? "";

            // Skip static assets
            var isStaticFile = path.Contains("/css/", StringComparison.OrdinalIgnoreCase)
                            || path.Contains("/js/", StringComparison.OrdinalIgnoreCase)
                            || path.Contains("/lib/", StringComparison.OrdinalIgnoreCase)
                            || path.Contains(".css", StringComparison.OrdinalIgnoreCase)
                            || path.Contains(".js", StringComparison.OrdinalIgnoreCase)
                            || path.Contains(".png", StringComparison.OrdinalIgnoreCase)
                            || path.Contains(".ico", StringComparison.OrdinalIgnoreCase)
                            || path.Contains(".jpg", StringComparison.OrdinalIgnoreCase)
                            || path.Contains(".webp", StringComparison.OrdinalIgnoreCase);

            if (isStaticFile)
            {
                await _next(context);
                return;
            }

            var isAuthenticated = context.User.Identity?.IsAuthenticated == true;
            var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;

            var isPatientPortalPath = path.StartsWith("/PatientPortal", StringComparison.OrdinalIgnoreCase);
            var isAuthPath = path.StartsWith("/Auth", StringComparison.OrdinalIgnoreCase);

            if (isAuthenticated)
            {
                if (userRole == "Patient")
                {
                    // Patient cannot access internal staff paths
                    if (!isPatientPortalPath && !isAuthPath && !path.Equals("/", StringComparison.OrdinalIgnoreCase))
                    {
                        var userName = context.User.Identity?.Name ?? "Patient";
                        
                        // Create ActivityLog for unauthorized access (EX6.1-05)
                        var log = new ActivityLog
                        {
                            Username = userName,
                            Action = "Truy cập trái phép",
                            Details = $"Bệnh nhân cố gắng truy cập địa chỉ nội bộ: {path}. Đã chặn và chuyển hướng."
                        };
                        dbContext.ActivityLogs.Add(log);
                        await dbContext.SaveChangesAsync();

                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.Redirect("/PatientPortal/Index");
                        return;
                    }
                }
                else
                {
                    // Internal staff cannot access patient portal paths
                    if (isPatientPortalPath)
                    {
                        context.Response.Redirect("/Home/Index");
                        return;
                    }
                }
            }
            else
            {
                // If not authenticated and trying to access patient portal, redirect to login
                if (isPatientPortalPath)
                {
                    context.Response.Redirect("/Auth/Login");
                    return;
                }
            }

            await _next(context);
        }
    }
}
