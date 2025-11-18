using MesEnterprise.Data;
using MesEnterprise.Models.Core;
using MesEnterprise.Models.Production;
using MesEnterprise.Models.Config;
using MesEnterprise.DTOs;
using MesEnterprise.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Collections.Concurrent;

namespace MesEnterprise.Endpoints
{
    public static class PublicEndpoints
    {
        private static readonly ConcurrentDictionary<string, ConcurrentQueue<(DateTime timestamp, int quantity)>> _scanBuffer = new();
        
        public static IEndpointRouteBuilder MapPublicApi(this IEndpointRouteBuilder app)
        {
            var scanApi = app.MapGroup("/api/scan").RequireAuthorization("OperatorOrHigher");
            ConfigureScanApi(scanApi);
            
            // Health check endpoint
            app.MapGet("/api/public/health", async (MesDbContext db) =>
            {
                try
                {
                    await db.Database.CanConnectAsync();
                    return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
                }
                catch (Exception ex)
                {
                    return Results.Json(new { status = "unhealthy", error = ex.Message }, statusCode: 503);
                }
            });
            
            return app;
        }
        
        private static void ConfigureScanApi(RouteGroupBuilder scanApi)
        {
{
    // GET /api/scan/status-by-identifier/{identifier}
    scanApi.MapGet("/status-by-identifier/{identifier}", [AllowAnonymous] async (string identifier, MesDbContext db) =>
    {
        var line = await db.Lines.FirstOrDefaultAsync(l => l.ScanIdentifier == identifier);
        if (line == null)
        {
            return Results.NotFound(new { Message = "Identificator stație invalid." });
        }

        if (!line.HasLiveScanning)
        {
            return Results.BadRequest(new { Message = "Scanarea live nu este activată pentru această linie." });
        }

        var status = await db.LineStatuses
            .Include(ls => ls.Product)
            .Include(ls => ls.CurrentShift)
            .FirstOrDefaultAsync(ls => ls.LineId == line.Id);

        if (status == null || status.Status != "Running")
        {
            return Results.Ok(new
            {
                LineId = line.Id,
                LineName = line.Name,
                Status = status?.Status ?? "Stopped",
                ProductName = "N/A"
            });
        }

        var now = DateTime.UtcNow;
        
        // ================== CORECȚIE BUG TIMP ==================
        var localTimeNow = TimeZoneInfo.ConvertTimeFromUtc(now, ApiHelpers.RomaniaTimeZone);
        var currentHourInterval = localTimeNow.ToString("HH:00"); // Folosim intervalul LOCAL
        // ================== SFÂRȘIT CORECȚIE ==================
        
        var dateToQuery = ApiHelpers.GetShiftDate(now, status.CurrentShift);

        var currentLog = await db.ProductionLogs.FirstOrDefaultAsync(pl =>
            pl.LineId == line.Id &&
            pl.ShiftId == status.CurrentShiftId &&
            pl.Timestamp.Date == dateToQuery &&
            pl.HourInterval == currentHourInterval); // Căutăm după intervalul LOCAL

        var shiftLogs = await db.ProductionLogs.Where(pl =>
            pl.LineId == line.Id &&
            pl.ShiftId == status.CurrentShiftId &&
            pl.Timestamp.Date == dateToQuery).ToListAsync();

        return Results.Ok(new
        {
            LineId = line.Id,
            LineName = line.Name,
            status.Status,
            status.ProductId,
            ProductName = status.Product?.Name,
            status.CurrentShiftId,
            ShiftName = status.CurrentShift?.Name,
            HourTotal = currentLog?.ActualParts ?? 0,
            ShiftTotal = shiftLogs.Sum(l => l.ActualParts)
        });
    });

    // POST /api/scan/log
    scanApi.MapPost("/log", [AllowAnonymous] async (ScanRequest req, MesDbContext db) =>
    {
        var line = await db.Lines.FirstOrDefaultAsync(l => l.ScanIdentifier == req.Identifier);
        if (line == null || !line.HasLiveScanning)
        {
            return Results.NotFound(new { Message = "Stație invalidă sau scanarea nu este activă." });
        }

        var lineStatus = await db.LineStatuses.FirstOrDefaultAsync(ls => ls.LineId == line.Id);
        if (lineStatus == null || lineStatus.Status != "Running" || !lineStatus.ProductId.HasValue || !lineStatus.CurrentShiftId.HasValue)
        {
            return Results.BadRequest(new { Message = "Linia este oprită sau nu este configurată." });
        }

        var productId = lineStatus.ProductId.Value;
        var shiftId = lineStatus.CurrentShiftId.Value;
        
        var now = DateTime.UtcNow;
        var shift = await db.Shifts.FindAsync(shiftId);
        var dateToQuery = ApiHelpers.GetShiftDate(now, shift);
        
        // ================== CORECȚIE BUG TIMP ==================
        var localTimeNow = TimeZoneInfo.ConvertTimeFromUtc(now, ApiHelpers.RomaniaTimeZone);
        var currentHourInterval = localTimeNow.ToString("HH:00"); // Folosim intervalul LOCAL
        // ================== SFÂRȘIT CORECȚIE ==================

        var log = await ApiHelpers.GetOrCreateProductionLog(db, line.Id, productId, shiftId, currentHourInterval, dateToQuery);

        if (req.ScanData == "GOOD")
        {
            log.ActualParts++;
        }
        else if (req.ScanData == "SCRAP")
        {
            log.ScrapParts++;
        }
        else if (req.ScanData == "NRFT")
        {
            log.NrftParts++;
        }
        else
        {
            return Results.BadRequest(new { Message = "Date scanate necunoscute." });
        }

        if (log.TargetParts == 0)
        {
            log.TargetParts = await ApiHelpers.CalculateTarget(db, line.Id, productId, shiftId, currentHourInterval, now, false);
        }
        
        log.Timestamp = now; 

        await db.SaveChangesAsync();

        var shiftLogs = await db.ProductionLogs.Where(pl =>
            pl.LineId == line.Id &&
            pl.ShiftId == shiftId &&
            pl.Timestamp.Date == dateToQuery).ToListAsync();

        return Results.Ok(new
        {
            Message = "Piesă înregistrată.",
            NewHourTotal = log.ActualParts,
            NewShiftTotal = shiftLogs.Sum(l => l.ActualParts)
        });
    });
}
        }
    }
}
