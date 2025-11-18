using MesEnterprise.Data;
using Microsoft.EntityFrameworkCore;

namespace MesEnterprise.Infrastructure
{
    /// <summary>
    /// Middleware to check if a module is enabled before allowing access
    /// </summary>
    public class ModuleGateMiddleware
    {
        private readonly RequestDelegate _next;

        public ModuleGateMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, MesDbContext db)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Define module paths and their corresponding settings
            var moduleChecks = new Dictionary<string, string>
            {
                { "/api/planning", "Production_Module_Enabled" },
                { "/api/maintenance-enterprise", "PM_Module_Enabled" },
                { "/api/quality", "Quality_Module_Enabled" },
                { "/api/inventory", "Inventory_Module_Enabled" },
                { "/api/analysis", "Analysis_Module_Enabled" },
                { "/api/export", "Export_Module_Enabled" },
                { "/api/live", "LiveView_Module_Enabled" }
            };

            foreach (var check in moduleChecks)
            {
                if (path.StartsWith(check.Key))
                {
                    var setting = await db.SystemSettings
                        .FirstOrDefaultAsync(s => s.Key == check.Value);
                    
                    if (setting == null || setting.Value != "true")
                    {
                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsJsonAsync(new { message = "Module is disabled" });
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
