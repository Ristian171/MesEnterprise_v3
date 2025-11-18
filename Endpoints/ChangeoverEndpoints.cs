using MesEnterprise.Data;
using MesEnterprise.Models.Core;
using MesEnterprise.Models.Production;
using MesEnterprise.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MesEnterprise.Endpoints
{
    public static class ChangeoverEndpoints
    {
        public static IEndpointRouteBuilder MapChangeoverApi(this IEndpointRouteBuilder app)
        {
            var changeoverApi = app.MapGroup("/api/changeover").RequireAuthorization("OperatorOrHigher");
            ConfigureChangeoverApi(changeoverApi);
            return app;
        }
        
        private static void ConfigureChangeoverApi(RouteGroupBuilder changeoverApi)
        {
{
    // POST /api/changeover/start
    changeoverApi.MapPost("/start", async ([FromBody] StartChangeoverRequest req, MesDbContext db, ClaimsPrincipal user) =>
    {
        var username = user.FindFirst(ClaimTypes.Name)?.Value;
        if (username == null) return Results.Unauthorized();

        var log = new ChangeoverLog
        {
            LineId = req.LineId,
            ProductFromId = req.ProductFromId,
            ProductToId = req.ProductToId,
            StartTime = DateTime.UtcNow 
        };
        db.ChangeoverLogs.Add(log);

        var lineStatus = await db.LineStatuses.FirstOrDefaultAsync(ls => ls.LineId == req.LineId);
        if (lineStatus != null && lineStatus.Status != "Breakdown")
        {
            await ApiHelpers.UpdateSystemDowntime(db, req.LineId, lineStatus.LastStatusChange, DateTime.UtcNow, lineStatus.Status == "Running");
            lineStatus.Status = "Changeover";
            lineStatus.ProductId = null;
            lineStatus.CurrentShiftId = null;
            lineStatus.LastStatusChange = DateTime.UtcNow; 
        }

        await db.SaveChangesAsync();
        return Results.Created($"/api/changeover/{log.Id}", new { Message = "Changeover început!", ChangeoverId = log.Id });
    });

    // POST /api/changeover/complete
    changeoverApi.MapPost("/complete", async ([FromBody] CompleteChangeoverRequest req, MesDbContext db) =>
    {
        var log = await db.ChangeoverLogs
            .Include(cl => cl.Line)
            .FirstOrDefaultAsync(cl => cl.Id == req.ChangeoverId);
        
        if (log == null)
            return Results.NotFound(new { Message = "Changeover not found" });

        log.EndTime = DateTime.UtcNow;
        var durationMinutes = (int)(log.EndTime.Value - log.StartTime).TotalMinutes;

        // Check if exceeded target
        var line = await db.Lines.FindAsync(log.LineId);
        bool exceededTarget = false;
        
        if (line != null && line.ChangeoverTargetMinutes.HasValue)
        {
            exceededTarget = durationMinutes > line.ChangeoverTargetMinutes.Value;
        }

        await db.SaveChangesAsync();

        return Results.Ok(new 
        { 
            Message = "Changeover finalizat!",
            DurationMinutes = durationMinutes,
            ExceededTarget = exceededTarget,
            TargetMinutes = line?.ChangeoverTargetMinutes,
            RequiresJustification = exceededTarget
        });
    });

    // GET /api/changeover/linestatuses
    changeoverApi.MapGet("/linestatuses", async (MesDbContext db) =>
    {
        var statuses = await db.LineStatuses
            .Include(ls => ls.Line)
            .Select(ls => new
            {
                ls.LineId,
                LineName = ls.Line.Name,
                ls.Status,
                CurrentProductId = ls.ProductId
            })
            .OrderBy(ls => ls.LineName)
            .ToListAsync();

        return Results.Ok(statuses);
    });


    // GET /api/changeover/history
    changeoverApi.MapGet("/history", async (MesDbContext db) =>
    {
        var history = await db.ChangeoverLogs
            .Include(cl => cl.Line)
            .Include(cl => cl.ProductFrom)
            .Include(cl => cl.ProductTo)
            .OrderByDescending(cl => cl.StartTime)
            .Take(50)
            .Select(cl => new
            {
                cl.Id,
                LineName = cl.Line.Name,
                ProductFromName = cl.ProductFrom.Name,
                ProductToName = cl.ProductTo.Name,
                cl.StartTime, 
                cl.EndTime    
            })
            .ToListAsync();

        return Results.Ok(history);
    });
}
        }
    }

    public record StartChangeoverRequest(int LineId, int ProductFromId, int ProductToId);
    public record CompleteChangeoverRequest(int ChangeoverId);
}
