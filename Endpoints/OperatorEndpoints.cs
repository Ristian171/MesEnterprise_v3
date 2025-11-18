/*
 * ===================================================================================
 * FIȘIER: MesSimplu/OperatorApi.cs
 * ROL: API pentru pagina operatorului.
 * STARE: MODIFICAT (CORECTAT BUG-URI CRITICE DB + FLUX AVARIE + GESTIONARE SESIUNE)
 *
 * MODIFICARE (Senior Dev):
 * - BUG CRITIC (DB): Rezolvat eroarea "division by zero" în GET /state.
 * Interogarea OEE folosește acum un operator ternar (CASE WHEN) pentru a preveni
 * împărțirea la un TargetParts = 0.
 * - BUG CRITIC (Stare): Eliminat `UserSessionManager` (dicționar static).
 * Toate endpoint-urile primesc acum lineId/productId din [FromQuery] de la client.
 * - BUG CRITIC (Timp): Rezolvat eroarea "DateTime Kind" în GET /history-preview.
 * Conversia UTC a fost corectată (folosea o variabilă deja convertită).
 * - FUNCȚIONALITATE: Adăugat endpoint pentru a prelua codurile de defect
 * necesare noului modal de logare.
 * ===================================================================================
 */

using MesEnterprise.Data;
using MesEnterprise.Models.Core;
using MesEnterprise.Models.Production;
using MesEnterprise.Models.Config;
using MesEnterprise.Models.Maintenance;
using MesEnterprise.DTOs;
using MesEnterprise.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security.Claims;

namespace MesEnterprise.Endpoints
{
    // DTO pentru noul endpoint de creare tichet
    public record CreateTicketRequest(int EquipmentId, int ProblemaId);

    public static class OperatorEndpoints
    {
        public static IEndpointRouteBuilder MapOperatorApi(this IEndpointRouteBuilder app)
        {
            var operatorApi = app.MapGroup("/api/operator").RequireAuthorization("OperatorOrHigher");

            // ================== ÎNCEPUT CORECȚIE BUG ÎNCĂRCARE PAGINĂ ==================
            // Endpoint-urile originale erau în ConfigApi și cereau rol de Admin.
            // Acestea sunt duplicate aici pentru a permite Operatorilor să încarce pagina.
            operatorApi.MapGet("/lines", async (MesDbContext db) =>
                await db.Lines
                    .Select(l => new { l.Id, l.Name })
                    .OrderBy(l => l.Name)
                    .ToListAsync());

            operatorApi.MapGet("/products", async (MesDbContext db) =>
                await db.Products
                    .Select(p => new { p.Id, p.Name, p.CycleTimeSeconds })
                    .OrderBy(p => p.Name)
                    .ToListAsync());
            // ================== SFÂRȘIT CORECȚIE BUG ÎNCĂRCARE PAGINĂ ==================

            // === NOU: Endpoint pentru a prelua categoriile și codurile de defecte ===
            operatorApi.MapGet("/defect-config", async (MesDbContext db) =>
            {
                var categories = await db.DefectCategories.OrderBy(c => c.Name).ToListAsync();
                var codes = await db.DefectCodes.OrderBy(c => c.Name).ToListAsync();
                return Results.Ok(new { categories, codes });
            });
            // =======================================================================


            // Setează sesiunea Linie/Produs pentru utilizator
            operatorApi.MapPost("/session", [Authorize("OperatorOrHigher")] async (
                [FromBody] StartProductionRequest req,
                MesDbContext db,
                ClaimsPrincipal user) =>
            {
                var username = user.FindFirst(ClaimTypes.Name)?.Value;
                if (username == null) return Results.Unauthorized();

                var line = await db.Lines.FindAsync(req.LineId);
                var product = await db.Products.FindAsync(req.ProductId);
                if (line == null || product == null)
                    return Results.BadRequest(new { Message = "Linia sau produsul selectat nu există." });

                var lineStatus = await db.LineStatuses.FirstOrDefaultAsync(ls => ls.LineId == req.LineId);
                if (lineStatus == null)
                {
                    lineStatus = new LineStatus { LineId = req.LineId, Status = "Stopped", ProductId = req.ProductId, LastStatusChange = DateTime.UtcNow }; 
                    db.LineStatuses.Add(lineStatus);
                }
                else
                {
                    lineStatus.ProductId = req.ProductId;
                    if (lineStatus.Status != "Stopped" && lineStatus.Status != "Breakdown")
                    {
                        await ApiHelpers.UpdateSystemDowntime(db, line.Id, lineStatus.LastStatusChange, DateTime.UtcNow, lineStatus.Status == "Running"); 
                        lineStatus.Status = "Stopped";
                        lineStatus.LastStatusChange = DateTime.UtcNow;
                    }
                    else
                    {
                        lineStatus.ProductId = req.ProductId;
                    }
                }

                // MODIFICARE: Sesiunea nu mai este stocată pe server
                // UserSessionManager.UserSessions[username] = (req.LineId, req.ProductId);

                await db.SaveChangesAsync();
                return Results.Ok(new { Message = "Sesiunea a fost setată." });
            });

            // GET /api/operator/state (Endpoint-ul principal pentru pagina index.html)
            operatorApi.MapGet("/state", [Authorize("OperatorOrHigher")] async (
                [FromQuery] int? lineId, 
                [FromQuery] int? productId, 
                MesDbContext db, 
                ClaimsPrincipal user) =>
            {
                var username = user.FindFirst(ClaimTypes.Name)?.Value;
                if (username == null) return Results.Unauthorized();

                // MODIFICARE: Validăm parametrii de interogare
                if (!lineId.HasValue || !productId.HasValue)
                {
                    return Results.Conflict(new { Message = "Sesiunea (Linia/Produsul) nu este setată." });
                }

                var line = await db.Lines.FindAsync(lineId.Value);
                if (line == null)
                {
                    return Results.Conflict(new { Message = "Linia din sesiune nu mai există. Vă rugăm reselectați." });
                }

                var lineStatus = await db.LineStatuses
                    .Include(ls => ls.Product)
                    .Include(ls => ls.CurrentShift)
                    .FirstOrDefaultAsync(ls => ls.LineId == line.Id);

                var now = DateTime.UtcNow; 

                if (lineStatus == null)
                {
                    lineStatus = new LineStatus { LineId = line.Id, Status = "Stopped", ProductId = productId.Value, LastStatusChange = now };
                    db.LineStatuses.Add(lineStatus);
                    await db.SaveChangesAsync();
                }

                if (lineStatus.ProductId != productId.Value && lineStatus.Status != "Changeover")
                {
                    lineStatus.ProductId = productId.Value;
                }
                
                // --- Logica de Schimb ---
                var localTimeNow = TimeZoneInfo.ConvertTimeFromUtc(now, ApiHelpers.RomaniaTimeZone); 
                var nowTimeOfDay = localTimeNow.TimeOfDay;
                // --- SFÂRȘIT ---

                var currentShift = await db.Shifts
                    .FirstOrDefaultAsync(s =>
                        (s.StartTime <= s.EndTime && s.StartTime <= nowTimeOfDay && s.EndTime > nowTimeOfDay) ||
                        (s.StartTime > s.EndTime && (s.StartTime <= nowTimeOfDay || s.EndTime > nowTimeOfDay))
                    );

                bool shiftChanged = false;
                if (currentShift != null && lineStatus.CurrentShiftId != currentShift.Id && lineStatus.Status != "Breakdown")
                {
                    lineStatus.CurrentShiftId = currentShift.Id;
                    shiftChanged = true;
                }
                else if (currentShift == null && lineStatus.Status != "Breakdown")
                {
                    lineStatus.CurrentShiftId = null;
                    shiftChanged = true;
                }
                
                if (shiftChanged)
                {
                    await db.SaveChangesAsync();
                }
                
                var dateToQuery = ApiHelpers.GetShiftDate(now, currentShift);
                var currentHourInterval = localTimeNow.ToString("HH:00");
                
                ProductionLog? currentHourLog = null;
                if (currentShift != null)
                {
                    currentHourLog = await db.ProductionLogs
                       .FirstOrDefaultAsync(pl =>
                           pl.LineId == line.Id &&
                           pl.ShiftId == currentShift.Id &&
                           pl.Timestamp.Date == dateToQuery &&
                           pl.HourInterval == currentHourInterval);
                }

                int hourlyTarget = 0;
                int realTimeTarget = 0;
                if (lineStatus.ProductId.HasValue && currentShift != null)
                {
                    hourlyTarget = await ApiHelpers.CalculateTarget(db, line.Id, lineStatus.ProductId.Value, currentShift.Id, currentHourInterval, now, false);
                    realTimeTarget = await ApiHelpers.CalculateTarget(db, line.Id, lineStatus.ProductId.Value, currentShift.Id, currentHourInterval, now, true);
                }

                List<ProductionLogDto> shiftLogs = new List<ProductionLogDto>();
                if (currentShift != null)
                {
                    var rawShiftLogs = await db.ProductionLogs
                        .Where(pl =>
                            pl.LineId == line.Id &&
                            pl.ShiftId == currentShift.Id &&
                            pl.Timestamp.Date == dateToQuery)
                        .Include(pl => pl.DeclaredDowntimeReason)
                        .OrderBy(pl => pl.HourInterval)
                        .Select(pl => new // Select raw data to calculate OEE in memory
                        {
                            Id = pl.Id,
                            StartTime = pl.Timestamp,
                            Target = pl.TargetParts, // Fetch TargetParts
                            Good = pl.ActualParts,   // Fetch ActualParts
                            Scrap = pl.ScrapParts,
                            // Oee will be calculated in memory to avoid EF Core translation bug
                            NeedsJustification = pl.JustificationRequired,
                            JustificationReasonName = pl.JustificationReason,
                            DeclaredDowntimeMinutes = pl.DeclaredDowntimeMinutes.GetValueOrDefault(0),
                            DeclaredDowntimeReasonName = pl.DeclaredDowntimeReason != null ? pl.DeclaredDowntimeReason.Name : null,
                            SystemStopMinutes = pl.SystemStopMinutes.GetValueOrDefault(0)
                        })
                        .ToListAsync();

                    // Perform OEE calculation in memory
                    shiftLogs = rawShiftLogs.Select(pl => new ProductionLogDto
                    {
                        Id = pl.Id,
                        StartTime = pl.StartTime,
                        EndTime = pl.StartTime.AddHours(1), // EndTime is derived from StartTime
                        Target = pl.Target,
                        Good = pl.Good,
                        Scrap = pl.Scrap,
                        Oee = pl.Target == 0 ? 0 : Math.Round(((double)pl.Good / pl.Target) * 100.0, 0),
                        NeedsJustification = pl.NeedsJustification,
                        JustificationReasonName = pl.JustificationReasonName,
                        DeclaredDowntimeMinutes = pl.DeclaredDowntimeMinutes,
                        DeclaredDowntimeReasonName = pl.DeclaredDowntimeReasonName,
                        SystemStopMinutes = pl.SystemStopMinutes
                    }).ToList();
                }

                var availableTimeSlots = ApiHelpers.GetAvailableTimeSlots(currentShift, now);

                // ================== ÎNCEPUT CORECȚIE "division by zero" #2 ==================
                // A doua eroare de "division by zero" era aici.
                double currentHourOEE = (hourlyTarget == 0) ? 0 : Math.Round(((double)(currentHourLog?.ActualParts ?? 0) / hourlyTarget) * 100, 0);
                // ================== SFÂRȘIT CORECȚIE "division by zero" #2 ==================

                var response = new OperatorStateDto
                {
                    LineId = line.Id,
                    LineName = line.Name,
                    LineStatus = lineStatus.Status,
                    ProductId = lineStatus.ProductId,
                    ProductName = lineStatus.Product?.Name,
                    ShiftId = currentShift?.Id,
                    ShiftName = currentShift?.Name,
                    ShiftStartTime = currentShift?.StartTime.ToString(@"hh\:mm"),
                    ShiftEndTime = currentShift?.EndTime.ToString(@"hh\:mm"),
                    CurrentHourTarget = hourlyTarget,
                    RealTimeTarget = realTimeTarget,
                    CurrentHourGoodParts = currentHourLog?.ActualParts ?? 0,
                    CurrentHourScrap = currentHourLog?.ScrapParts ?? 0,
                    CurrentHourOEE = currentHourOEE,
                    // Live Scan info
                    ScanIdentifier = line.ScanIdentifier,
                    LiveScanAvailable = line.HasLiveScanning,
                    LiveScanEnabled = line.LiveScanEnabled,
                    // OEE Target
                    OeeTarget = line.OeeTarget,
                    AvailableTimeSlots = availableTimeSlots,
                    CurrentShiftLogs = shiftLogs
                };

                return Results.Ok(response);
            });

            // POST /api/operator/command (Start, Stop)
            operatorApi.MapPost("/command", [Authorize("OperatorOrHigher")] async (
                [FromBody] OperatorCommandRequest req,
                [FromQuery] int? lineId, 
                [FromQuery] int? productId, 
                MesDbContext db,
                ClaimsPrincipal user) =>
            {
                var username = user.FindFirst(ClaimTypes.Name)?.Value;
                if (username == null) return Results.Unauthorized();

                if (!lineId.HasValue)
                    return Results.Conflict(new { Message = "Sesiunea nu este setată. Selectați linia/produsul." });

                var line = await db.Lines.FindAsync(lineId.Value);
                if (line == null) return Results.NotFound(new { Message = "Linia din sesiune nu a fost găsită." });

                var lineStatus = await db.LineStatuses.FirstOrDefaultAsync(ls => ls.LineId == line.Id);
                if (lineStatus == null) return Results.NotFound(new { Message = "Starea liniei nu a fost găsită." });
                
                string message = "";
                var now = DateTime.UtcNow; 

                switch (req.Command.ToLower())
                {
                    case "start":
                        if (lineStatus.Status == "Stopped")
                        {
                            if (!lineStatus.ProductId.HasValue)
                                return Results.BadRequest(new { Message = "Nu se poate porni. Selectați un produs mai întâi." });

                            lineStatus.Status = "Running";
                            await ApiHelpers.UpdateSystemDowntime(db, line.Id, lineStatus.LastStatusChange, now, false);
                            lineStatus.LastStatusChange = now;
                            message = "Producția a pornit.";
                        }
                        break;

                    case "stop":
                        if (lineStatus.Status == "Running")
                        {
                            lineStatus.Status = "Stopped";
                            await ApiHelpers.UpdateSystemDowntime(db, line.Id, lineStatus.LastStatusChange, now, true);
                            lineStatus.LastStatusChange = now;
                            message = "Producția a fost oprită.";
                        }
                        break;
                    
                    case "breakdown":
                        if (lineStatus.Status != "Breakdown")
                        {
                            bool wasRunning = lineStatus.Status == "Running";
                            await ApiHelpers.UpdateSystemDowntime(db, line.Id, lineStatus.LastStatusChange, now, wasRunning);

                            lineStatus.Status = "Breakdown";
                            lineStatus.LastStatusChange = now;
                            message = "Avarie raportată (fallback). Linia a fost oprită.";
                        }
                        break;

                    default:
                        return Results.BadRequest(new { Message = "Comandă necunoscută." });
                }

                await db.SaveChangesAsync();
                return Results.Ok(new { Message = message });
            });

            // POST /api/operator/create-ticket (Conectează butonul "Raportează Avarie")
            operatorApi.MapPost("/create-ticket", [Authorize("OperatorOrHigher")] async (
                [FromBody] CreateTicketRequest req,
                [FromQuery] int? lineId, 
                [FromQuery] int? productId, 
                MesDbContext db,
                ClaimsPrincipal user) =>
            {
                var username = user.FindFirst(ClaimTypes.Name)?.Value;
                if (username == null) return Results.Unauthorized();

                if (!lineId.HasValue || !productId.HasValue)
                    return Results.Conflict(new { Message = "Sesiunea nu este setată." });
                
                var equipment = await db.Equipments.FindAsync(req.EquipmentId);
                if (equipment == null || equipment.LineId != lineId.Value)
                    return Results.BadRequest(new { Message = "Echipamentul selectat este invalid pentru această linie." });
                
                var lineStatus = await db.LineStatuses.FirstOrDefaultAsync(ls => ls.LineId == lineId.Value);
                if (lineStatus == null) 
                    return Results.NotFound(new { Message = "Starea liniei nu a fost găsită." });

                var now = DateTime.UtcNow;

                if (lineStatus.Status != "Breakdown")
                {
                    bool wasRunning = lineStatus.Status == "Running";
                    await ApiHelpers.UpdateSystemDowntime(db, lineId.Value, lineStatus.LastStatusChange, now, wasRunning);
                    
                    lineStatus.Status = "Breakdown";
                    lineStatus.LastStatusChange = now;
                }

                var tichet = new InterventieTichet
                {
                    UnicIdTicket = Guid.NewGuid(),
                    Status = "Deschis",
                    LineId = lineId.Value,
                    EquipmentId = req.EquipmentId,
                    ProductId = productId.Value, 
                    OperatorNume = username,
                    DataRaportareOperator = now,
                    ProblemaRaportataId = req.ProblemaId
                };

                db.InterventieTichete.Add(tichet);
                await db.SaveChangesAsync();

                return Results.Ok(new { Message = "Avarie raportată cu succes. Tichetul a fost creat." });
            });


            // Salvează o observație ad-hoc
            operatorApi.MapPost("/observatie", [Authorize("OperatorOrHigher")] async (
                [FromBody] ObservatieOperatorRequest req,
                [FromQuery] int? lineId, 
                [FromQuery] int? productId, 
                MesDbContext db,
                ClaimsPrincipal user) =>
            {
                var username = user.FindFirst(ClaimTypes.Name)?.Value;
                var userIdStr = user.FindFirst("UserId")?.Value;

                if (username == null || !int.TryParse(userIdStr, out var userId))
                    return Results.Unauthorized();

                if (!lineId.HasValue || !productId.HasValue)
                    return Results.Conflict(new { Message = "Sesiunea (Linia/Produsul) nu este setată." });

                if (string.IsNullOrWhiteSpace(req.Text))
                    return Results.BadRequest(new { Message = "Textul observației nu poate fi gol." });

                var observatie = new ObservatieOperator
                {
                    Text = req.Text,
                    DataOra = DateTime.UtcNow,
                    LineId = lineId.Value,
                    ProductId = productId.Value,
                    UserId = userId
                };

                db.ObservatiiOperator.Add(observatie);
                await db.SaveChangesAsync();

                return Results.Ok(new { Message = "Observație salvată." });
            });

            // Returnează preview-ul de istoric (Logări + Observații)
            operatorApi.MapGet("/history-preview", [Authorize("OperatorOrHigher")] async (
                [FromQuery] int? lineId, 
                [FromQuery] int? productId, 
                MesDbContext db,
                ClaimsPrincipal user) =>
            {
                var username = user.FindFirst(ClaimTypes.Name)?.Value;
                if (username == null) return Results.Unauthorized();

                if (!lineId.HasValue)
                    return Results.Ok(new List<object>());

                var lineStatus = await db.LineStatuses.Include(ls => ls.CurrentShift)
                                   .FirstOrDefaultAsync(ls => ls.LineId == lineId.Value);

                if (lineStatus?.CurrentShiftId == null)
                    return Results.Ok(new List<object>());

                var shift = lineStatus.CurrentShift;
                var now = DateTime.UtcNow;
                var localTimeNow = TimeZoneInfo.ConvertTimeFromUtc(now, ApiHelpers.RomaniaTimeZone);
                var dateToQuery = ApiHelpers.GetShiftDate(now, shift); 
                
                var localShiftStartDate = dateToQuery;
                
                // ================== ÎNCEPUT CORECȚIE "DateTime Kind" ==================
                // 'dateToQuery' este acum UTC. 'localShiftStartDate' este UTC.
                // 'StartTime' este un TimeSpan. Adunarea lor păstrează Kind-ul UTC.
                var localShiftStartTime = localShiftStartDate.Add(shift!.StartTime);

                if (shift.StartTime > shift.EndTime && localTimeNow.TimeOfDay < shift.EndTime) 
                {
                    localShiftStartTime = localShiftStartTime.AddDays(-1);
                }
                
                // AICI A FOST BUG-UL: 'localShiftStartTime' este deja UTC.
                // Nu mai este nevoie de 'ConvertTimeToUtc'.
                var shiftStartDateTimeUtc = localShiftStartTime;
                // ================== SFÂRȘIT CORECȚIE "DateTime Kind" ==================


                var logs = await db.ProductionLogs
                    .Where(pl => pl.LineId == lineId.Value && pl.ShiftId == shift!.Id && pl.Timestamp >= shiftStartDateTimeUtc)
                    .OrderByDescending(pl => pl.Timestamp)
                    .Take(5)
                    .Select(pl => new
                    {
                        Type = "log",
                        Timestamp = pl.Timestamp, 
                        HourInterval = pl.HourInterval,
                        Good = pl.ActualParts,
                        Scrap = pl.ScrapParts,
                        Nrft = pl.NrftParts,
                        Text = "Logare orară"
                    })
                    .ToListAsync();

                var observatii = await db.ObservatiiOperator
                    .Where(o => o.LineId == lineId.Value && o.DataOra >= shiftStartDateTimeUtc)
                    .OrderByDescending(o => o.DataOra)
                    .Take(5)
                    .Select(o => new
                    {
                        Type = "obs",
                        Timestamp = o.DataOra, 
                        HourInterval = o.DataOra.ToString("HH:mm"),
                        Good = 0,
                        Scrap = 0,
                        Nrft = 0,
                        Text = o.Text
                    })
                    .ToListAsync();

                var combined = logs.Cast<object>().Concat(observatii.Cast<object>())
                    .OrderByDescending(x => (DateTime)x.GetType().GetProperty("Timestamp")!.GetValue(x)!)
                    .Take(5);

                return Results.Ok(combined);
            });

            return app;
        }
    }
}