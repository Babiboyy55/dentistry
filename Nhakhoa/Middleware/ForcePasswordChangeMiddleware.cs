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
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst("UserId")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var user = await dbContext.Users.FindAsync(userId);
                    if (user != null)
                    {
                        // 1. Force logout if SecurityStamp claim doesn't match database
                        var securityStampClaim = context.User.FindFirst("SecurityStamp")?.Value;
                        if (securityStampClaim != user.SecurityStamp)
                        {
                            await context.SignOutAsync("Cookies");
                            context.Response.Redirect("/Auth/Login");
                            return;
                        }

                        // 2. Force password change if user has a temporary password
                        if (user.IsTemporaryPassword)
                        {
                            var path = context.Request.Path.Value ?? "";
                            if (!path.Equals("/Auth/ChangePassword", StringComparison.OrdinalIgnoreCase) &&
                                !path.Equals("/Auth/Logout", StringComparison.OrdinalIgnoreCase) &&
                                !path.Contains("/css/", StringComparison.OrdinalIgnoreCase) &&
                                !path.Contains("/js/", StringComparison.OrdinalIgnoreCase) &&
                                !path.Contains("/lib/", StringComparison.OrdinalIgnoreCase) &&
                                !path.Contains("/Nhakhoa.styles.css", StringComparison.OrdinalIgnoreCase))
                            {
                                context.Response.Redirect("/Auth/ChangePassword");
                                return;
                            }
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
