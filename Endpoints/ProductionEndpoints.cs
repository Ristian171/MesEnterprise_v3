using MesEnterprise.Data;
using MesEnterprise.Models.Core;
using MesEnterprise.Models.Production;
using MesEnterprise.Models.Config;
using MesEnterprise.Models.Quality;
using MesEnterprise.DTOs;
using MesEnterprise.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Globalization;

namespace MesEnterprise.Endpoints
{
    public static class ProductionEndpoints
    {
        public static IEndpointRouteBuilder MapProductionApi(this IEndpointRouteBuilder app)
        {
            var productionLogApi = app.MapGroup("/api/productionlogs").RequireAuthorization("OperatorOrHigher");
            var editApi = app.MapGroup("/api/edit").RequireAuthorization("AdminOnly");
            
            ConfigureProductionLogApi(productionLogApi);
            ConfigureEditApi(editApi);
            ConfigureLiveScanApi(app);
            
            return app;
        }
        
        private static void ConfigureLiveScanApi(IEndpointRouteBuilder app)
        {
            // POST /api/production/scan - Live Scan endpoint
            app.MapPost("/api/production/scan", async (
                [FromBody] ScanRequest request,
                MesDbContext db) =>
            {
                if (string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrWhiteSpace(request.ScanData))
                {
                    return Results.BadRequest(new { Message = "Identifier și ScanData sunt obligatorii." });
                }

                // Find line by ScanIdentifier
                var line = await db.Lines
                    .FirstOrDefaultAsync(l => l.ScanIdentifier == request.Identifier && l.IsActive);

                if (line == null)
                {
                    return Results.BadRequest(new { Message = "Stație scan neconfigurată pentru acest identificator." });
                }

                // Check if Live Scan is available (configured by Admin)
                if (!line.HasLiveScanning)
                {
                    return Results.BadRequest(new { Message = "Stație scan neconfigurată pentru această linie." });
                }

                // Check if Live Scan is currently enabled (controlled by Operator)
                if (!line.LiveScanEnabled)
                {
                    return Results.Ok(new 
                    { 
                        Message = "Live Scan dezactivat. Scan înregistrat dar nu a fost procesat.", 
                        Action = "ignored",
                        LineId = line.Id,
                        LineName = line.Name
                    });
                }

                // Get current line status
                var lineStatus = await db.LineStatuses
                    .FirstOrDefaultAsync(ls => ls.LineId == line.Id);

                if (lineStatus == null || lineStatus.ProductId == null)
                {
                    return Results.BadRequest(new { Message = "Linia nu are un produs activ. Configurați produsul din pagina de operator." });
                }

                if (lineStatus.CurrentShiftId == null)
                {
                    return Results.BadRequest(new { Message = "Linia nu este într-un schimb activ." });
                }

                var productId = lineStatus.ProductId.Value;
                var shiftId = lineStatus.CurrentShiftId.Value;

                // Get current time and interval
                var nowUtc = DateTime.UtcNow;
                var localNow = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, ApiHelpers.RomaniaTimeZone);
                var hourInterval = localNow.ToString("HH:00");

                var shift = await db.Shifts.FindAsync(shiftId);
                var dateToQuery = ApiHelpers.GetShiftDate(nowUtc, shift);

                // Get or create production log for current hour
                var productionLog = await ApiHelpers.GetOrCreateProductionLog(
                    db, line.Id, productId, shiftId, hourInterval, dateToQuery);

                // Increment good parts by 1
                productionLog.ActualParts += 1;

                // Recalculate target
                productionLog.TargetParts = await ApiHelpers.CalculateTarget(
                    db, line.Id, productId, shiftId, hourInterval, nowUtc, false);

                await db.SaveChangesAsync();

                return Results.Ok(new
                {
                    Message = "Piesă bună înregistrată prin Live Scan.",
                    Action = "good_part_added",
                    LineId = line.Id,
                    LineName = line.Name,
                    ProductId = productId,
                    GoodParts = productionLog.ActualParts,
                    TargetParts = productionLog.TargetParts,
                    HourInterval = hourInterval
                });
            });

            // GET /api/line/status-by-identifier/{identifier} - Get line status by scan identifier
            app.MapGet("/api/line/status-by-identifier/{identifier}", async (
                string identifier,
                MesDbContext db) =>
            {
                var line = await db.Lines
                    .Include(l => l.Department)
                    .FirstOrDefaultAsync(l => l.ScanIdentifier == identifier && l.IsActive);

                if (line == null)
                {
                    return Results.NotFound(new { Message = "Linie negăsită pentru acest identificator." });
                }

                var lineStatus = await db.LineStatuses
                    .Include(ls => ls.Product)
                    .Include(ls => ls.CurrentShift)
                    .FirstOrDefaultAsync(ls => ls.LineId == line.Id);

                return Results.Ok(new
                {
                    Line = new
                    {
                        line.Id,
                        line.Name,
                        line.ScanIdentifier,
                        line.HasLiveScanning,
                        LiveScanAvailable = line.HasLiveScanning,
                        line.LiveScanEnabled,
                        Department = line.Department?.Name
                    },
                    Status = lineStatus == null ? null : new
                    {
                        lineStatus.Status,
                        lineStatus.LastStatusChange,
                        Product = lineStatus.Product == null ? null : new
                        {
                            lineStatus.Product.Id,
                            lineStatus.Product.Name
                        },
                        Shift = lineStatus.CurrentShift == null ? null : new
                        {
                            lineStatus.CurrentShift.Id,
                            lineStatus.CurrentShift.Name
                        }
                    }
                });
            });

            // PUT /api/line/{lineId}/live-scan - Enable/disable Live Scan for a line
            app.MapPut("/api/line/{lineId}/live-scan", async (
                int lineId,
                [FromBody] LiveScanToggleRequest request,
                MesDbContext db) =>
            {
                var line = await db.Lines.FindAsync(lineId);
                if (line == null)
                {
                    return Results.NotFound(new { Message = "Linie negăsită." });
                }

                if (!line.HasLiveScanning)
                {
                    return Results.BadRequest(new { Message = "Live Scan nu este disponibil pentru această linie. Configurați în pagina de administrare." });
                }

                line.LiveScanEnabled = request.Enabled;
                await db.SaveChangesAsync();

                return Results.Ok(new
                {
                    Message = request.Enabled 
                        ? "Live Scan activat pentru linia " + line.Name
                        : "Live Scan dezactivat pentru linia " + line.Name,
                    LineId = line.Id,
                    LineName = line.Name,
                    LiveScanEnabled = line.LiveScanEnabled
                });
            }).RequireAuthorization("OperatorOrHigher");
        }
        
        private static void ConfigureProductionLogApi(RouteGroupBuilder productionLogApi)
        {
{
    // POST /api/productionlogs
    productionLogApi.MapPost("/", [Authorize("OperatorOrHigher")] async (
        [FromBody] ProductionLogRequest req,
        [FromQuery] int? lineIdQuery, // MODIFICARE: Primit de la client
        [FromQuery] int? productIdQuery, // MODIFICARE: Primit de la client
        MesDbContext db,
        ClaimsPrincipal user) =>
    {
        var username = user.FindFirst(ClaimTypes.Name)?.Value;
        if (username == null) return Results.Unauthorized();

        // MODIFICARE: Am eliminat UserSessionManager
        if (!lineIdQuery.HasValue || !productIdQuery.HasValue)
        {
            return Results.Conflict(new { Message = "Sesiunea (Linia/Produsul) nu este setată." });
        }
        
        var lineId = lineIdQuery.Value;
        var productId = productIdQuery.Value;

        var lineStatus = await db.LineStatuses.FirstOrDefaultAsync(ls => ls.LineId == lineId);
        if (lineStatus?.CurrentShiftId == null)
            return Results.BadRequest(new { Message = "Linia nu este într-un schimb activ." });

        var shiftId = lineStatus.CurrentShiftId.Value;

        DateTime logTimeUtc;
        if (!DateTime.TryParse(req.LogTime, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out logTimeUtc))
        {
            return Results.BadRequest(new { Message = "Formatul orei este invalid." });
        }
        logTimeUtc = DateTime.SpecifyKind(logTimeUtc, DateTimeKind.Utc);
        
        // ================== CORECȚIE BUG TIMP ==================
        // Convertim ora UTC la ora locală PENTRU A OBȚINE INTERVALUL
        var localLogTime = TimeZoneInfo.ConvertTimeFromUtc(logTimeUtc, ApiHelpers.RomaniaTimeZone);
        var hourInterval = localLogTime.ToString("HH:00"); // Folosim intervalul LOCAL
        // ================== SFÂRȘIT CORECȚIE ==================

        var shift = await db.Shifts.FindAsync(shiftId);
        // GetShiftDate folosește 'logTimeUtc' (corect) pentru a găsi data
        var dateToQuery = ApiHelpers.GetShiftDate(logTimeUtc, shift); 
        
        var settings = await db.SystemSettings.ToDictionaryAsync(s => s.Key, s => s.Value);
        string logModeBune = settings.GetValueOrDefault("GoodPartsLoggingMode", "Overwrite");
        string logModeDefecte = settings.GetValueOrDefault("DowntimeScrapLoggingMode", "Overwrite");

        // Folosim hourInterval (LOCAL) pentru a găsi log-ul
        var existingLog = await ApiHelpers.GetOrCreateProductionLog(db, lineId, productId, shiftId, hourInterval, dateToQuery);

        // Trimitem ora UTC 'logTimeUtc' (corect)
        // Trimitem hourInterval (LOCAL) (corect)
        existingLog.TargetParts = await ApiHelpers.CalculateTarget(db, lineId, productId, shiftId, hourInterval, logTimeUtc, false);

        if (logModeBune == "Aggregate")
        {
            existingLog.ActualParts += req.GoodParts;
        }
        else
        {
            existingLog.ActualParts = req.GoodParts;
        }

        if (logModeDefecte == "Aggregate")
        {
            existingLog.ScrapParts += req.ScrapParts;
            existingLog.NrftParts += req.NrftParts;
            existingLog.DeclaredDowntimeMinutes = (existingLog.DeclaredDowntimeMinutes ?? 0) + req.DeclaredDowntimeMinutes;

            if (req.DeclaredDowntimeReasonId.HasValue && !existingLog.DeclaredDowntimeReasonId.HasValue)
            {
                existingLog.DeclaredDowntimeReasonId = req.DeclaredDowntimeReasonId;
            }
        }
        else
        {
            existingLog.ScrapParts = req.ScrapParts;
            existingLog.NrftParts = req.NrftParts;
            existingLog.DeclaredDowntimeMinutes = req.DeclaredDowntimeMinutes;
            existingLog.DeclaredDowntimeReasonId = req.DeclaredDowntimeReasonId;

            var oldDefects = await db.ProductionLogDefects.Where(pld => pld.ProductionLogId == existingLog.Id).ToListAsync();
            if (oldDefects.Any())
            {
                db.ProductionLogDefects.RemoveRange(oldDefects);
            }
        }

        if (req.Defecte != null && req.Defecte.Any())
        {
            foreach (var defect in req.Defecte)
            {
                if (defect.Quantity > 0)
                {
                    ProductionLogDefect? existingDefectAllocation = null;
                    if (logModeDefecte == "Aggregate")
                    {
                        existingDefectAllocation = await db.ProductionLogDefects
                           .FirstOrDefaultAsync(pld => pld.ProductionLogId == existingLog.Id && pld.DefectCodeId == defect.DefectCodeId);
                    }
                    if (existingDefectAllocation != null)
                    {
                        existingDefectAllocation.Quantity += defect.Quantity;
                    }
                    else
                    {
                        db.ProductionLogDefects.Add(new ProductionLogDefect
                        {
                            ProductionLog = existingLog,
                            DefectCodeId = defect.DefectCodeId,
                            Quantity = defect.Quantity
                        });
                    }
                }
            }
        }

        existingLog.Timestamp = logTimeUtc; 

        bool requireJustification = bool.TryParse(settings.GetValueOrDefault("RequireJustification", "true"), out var r) ? r : true;

        if (requireJustification)
        {
            double targetPercent = double.TryParse(settings.GetValueOrDefault("JustificationThresholdPercent", "85"), out var p) ? p : 85;
            double oee = (existingLog.TargetParts == 0) ? 100 : ((double)existingLog.ActualParts / existingLog.TargetParts) * 100.0;

            bool isJustifiedByDowntime = (req.DeclaredDowntimeMinutes > 0 || req.DeclaredDowntimeReasonId.HasValue);

            if (oee < targetPercent && !isJustifiedByDowntime)
            {
                existingLog.JustificationRequired = true;
                if (logModeDefecte == "Overwrite")
                {
                    existingLog.JustificationReason = null;
                }
            }
            else
            {
                existingLog.JustificationRequired = false;
                if (isJustifiedByDowntime)
                {
                    var reason = await db.BreakdownReasons.FindAsync(req.DeclaredDowntimeReasonId);
                    existingLog.JustificationReason = reason?.Name ?? "Staționare declarată";
                }
                else if (oee >= targetPercent)
                {
                    existingLog.JustificationReason = null;
                }
            }
        }
        else
        {
            existingLog.JustificationRequired = false;
        }

        await db.SaveChangesAsync();

        if ((existingLog.ScrapParts > 0 || existingLog.NrftParts > 0) && lineStatus.Status != "Breakdown")
        {
            var stopResult = await ApiHelpers.CheckStopOnDefectRules(db, lineStatus, existingLog, username);
            if (stopResult.ShouldStop)
            {
                return Results.Conflict(new
                {
                    StopAlert = true,
                    Message = stopResult.Reason
                });
            }
        }

        return Results.Ok(new { Message = "Logare salvată cu succes!" });
    });

    // POST /api/productionlogs/{id}/justify
    productionLogApi.MapPost("/{logId}/justify", [Authorize("OperatorOrHigher")] async (
        int logId,
        [FromBody] JustificationRequest req,
        MesDbContext db,
        ClaimsPrincipal user) =>
    {
        var log = await db.ProductionLogs.FindAsync(logId);
        if (log == null)
        {
            return Results.NotFound(new { Message = "Log-ul orar nu a fost găsit." });
        }

        var reason = await db.BreakdownReasons.FindAsync(req.BreakdownReasonId);
        if (reason == null)
        {
            return Results.BadRequest(new { Message = "Motivul selectat este invalid." });
        }

        log.JustificationReason = reason.Name + (string.IsNullOrEmpty(req.Comments) ? "" : $" ({req.Comments})");
        log.JustificationRequired = false;

        await db.SaveChangesAsync();
        return Results.Ok(new { Message = "Justificare salvată." });
    });
}
        }
        
        private static void ConfigureEditApi(RouteGroupBuilder editApi)
        {
{
    // GET /api/edit/logs-by-date
    editApi.MapGet("/logs-by-date", async (int lineId, DateTime date, MesDbContext db) =>
    {
        var targetDate = date.Date;
        var logs = await db.ProductionLogs
            .Where(pl => pl.LineId == lineId && pl.Timestamp.Date == targetDate) 
            .Include(pl => pl.Product)
            .Include(pl => pl.Shift)
            .OrderBy(pl => pl.HourInterval)
            .Select(pl => new
            {
                pl.Id,
                pl.HourInterval,
                ProductName = pl.Product != null ? pl.Product.Name : "N/A",
                ShiftName = pl.Shift != null ? pl.Shift.Name : "N/A",
                pl.TargetParts,
                pl.ActualParts,
                pl.ScrapParts,
                pl.NrftParts,
                pl.Timestamp 
            })
            .ToListAsync();

        return Results.Ok(logs);
    });

    // PUT /api/edit/log/{logId}
    editApi.MapPut("/log/{logId}", async (int logId, [FromBody] ProductionLogEditRequest req, MesDbContext db) =>
    {
        var log = await db.ProductionLogs.FindAsync(logId);
        if (log == null)
        {
            return Results.NotFound();
        }

        log.ActualParts = req.ActualParts;
        log.ScrapParts = req.ScrapParts;
        log.NrftParts = req.NrftParts;
        log.JustificationRequired = false;
        log.JustificationReason = "Editat Manual Admin";
        
        await db.SaveChangesAsync();
        return Results.Ok(log);
    });

    // DELETE /api/edit/log/{logId}
    editApi.MapDelete("/log/{logId}", async (int logId, MesDbContext db) =>
    {
        var log = await db.ProductionLogs.FindAsync(logId);
        if (log == null)
        {
            return Results.NotFound();
        }

        var defects = await db.ProductionLogDefects.Where(pld => pld.ProductionLogId == logId).ToListAsync();
        if (defects.Any())
        {
            db.ProductionLogDefects.RemoveRange(defects);
        }

        db.ProductionLogs.Remove(log);
        await db.SaveChangesAsync();
        return Results.NoContent();
    });
}

        }
    }
}
