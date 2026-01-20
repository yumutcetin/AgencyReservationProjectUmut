using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Middleware
{
    public class ForcePasswordChangeMiddleware
    {
        private readonly RequestDelegate _next;

        public ForcePasswordChangeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
        {
            var path = context.Request.Path;

            // 1) Statik dosyaları serbest bırak
            if (IsStatic(path))
            {
                await _next(context);
                return;
            }

            // 2) Kimliği doğrulanmamış istekleri serbest bırak
            if (context.User.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            // 3) [AllowAnonymous] endpoint veya whitelist yolları serbest bırak
            if (IsWhitelistedPath(context, path))
            {
                await _next(context);
                return;
            }

            // 4) Kullanıcıyı yükle ve kontrol et
            var user = await userManager.GetUserAsync(context.User);
            if (user is null)
            {
                await _next(context);
                return;
            }

            var mustChangePassword =
                user.LastPasswordChangeDate == null ||
                (DateTime.UtcNow - user.LastPasswordChangeDate.Value).TotalDays > 90;

            if (mustChangePassword)
            {
                context.Response.Redirect("/Identity/Account/ForceChangePassword");
                return;
            }

            await _next(context);
        }

        private static bool IsStatic(PathString path)
        {
            if (!path.HasValue) return false;
            var p = path.Value!;
            return p.StartsWith("/css", StringComparison.OrdinalIgnoreCase) ||
                   p.StartsWith("/js", StringComparison.OrdinalIgnoreCase) ||
                   p.StartsWith("/lib", StringComparison.OrdinalIgnoreCase) ||
                   p.StartsWith("/dist", StringComparison.OrdinalIgnoreCase) ||
                   p.StartsWith("/images", StringComparison.OrdinalIgnoreCase) ||
                   p.StartsWith("/img", StringComparison.OrdinalIgnoreCase) ||
                   p.StartsWith("/fonts", StringComparison.OrdinalIgnoreCase) ||
                   p.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase) ||
                   p.EndsWith(".css", StringComparison.OrdinalIgnoreCase) ||
                   p.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
                   p.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                   p.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                   p.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                   p.EndsWith(".svg", StringComparison.OrdinalIgnoreCase) ||
                   p.EndsWith(".woff", StringComparison.OrdinalIgnoreCase) ||
                   p.EndsWith(".woff2", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsWhitelistedPath(HttpContext context, PathString path)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
                return true;

            // allow exact and sub-routes
            return
                path.StartsWithSegments("/Identity/Account/ForceChangePassword", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWithSegments("/Identity/Account/Login", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWithSegments("/Identity/Account/Logout", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWithSegments("/Identity/Account/AccessDenied", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWithSegments("/Home/Login", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWithSegments("/Home/Logout", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWithSegments("/Error", StringComparison.OrdinalIgnoreCase);
        }

    }
}
