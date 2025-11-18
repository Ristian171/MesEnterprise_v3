/*
 * ===================================================================================
 * FIȘIER: MesSimplu/Api/ApiHelpers.cs
 * ROL: Funcții helper centralizate.
 * STARE: MODIFICAT (CORECTAT BUG CRITIC TIMP)
 *
 * MODIFICARE (Senior Dev):
 * - CORECTAT EROARE CS0122: 'RomaniaTimeZone' a fost schimbat din 'private'
 * (implicit) în 'public' pentru a fi accesibil de către alte clase (OperatorApi).
 * - CORECTAT BUG CRITIC (DateTime Kind): GetShiftDate() returna o dată
 * cu Kind=Unspecified, provocând o eroare Npgsql la interogarea log-urilor.
 * - CORECȚIE: Funcția returnează acum explicit data cu DateTimeKind.Utc,
 * permițând bazei de date să execute corect interogarea.
 * ===================================================================================
 */

using MesEnterprise.Data;
using MesEnterprise.Models.Core;
using MesEnterprise.Models.Production;
using MesEnterprise.Models.Maintenance;
using MesEnterprise.Models.Config;
using MesEnterprise.Models.Quality;
using MesEnterprise.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MesEnterprise.Services
{
    public static class ApiHelpers
    {
        // Definim fusul orar al României (pentru servere Windows)
        // Alternativ, pentru Linux: "Europe/Bucharest"
        private const string RomaniaTimeZoneId = "E. Europe Standard Time"; 
        
        // MODIFICARE: Făcut 'public' pentru a fi accesibil din OperatorApi
        public static readonly TimeZoneInfo RomaniaTimeZone = GetRomaniaTimeZone();

        // MODIFICARE: Făcut 'public'
        public static TimeZoneInfo GetRomaniaTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(RomaniaTimeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback pentru medii non-Windows (Linux/Docker)
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById("Europe/Bucharest");
                }
                catch (Exception)
                {
                    // Fallback extrem: Folosește EEST (dar fără DST)
                    return TimeZoneInfo.CreateCustomTimeZone("EEST", TimeSpan.FromHours(2), "EEST", "EEST");
                }
            }
        }


        /// <summary>
        /// Calculează ținta pentru un interval orar, scăzând pauzele și timpii dummy.
        /// 'now' ESTE AȘTEPTAT SĂ FIE ORA UTC (DateTime.UtcNow)
        /// </summary>
        public static async Task<int> CalculateTarget(MesDbContext db, int lineId, int productId, int shiftId, string hourInterval, DateTime now, bool realTimeOnly = false)
        {
            var product = await db.Products.FindAsync(productId);
            if (product == null || product.CycleTimeSeconds == 0) return 0;

            double totalMinutesInInterval = 60.0;
            TimeSpan intervalStart = TimeSpan.Parse(hourInterval); // Acesta este un interval UTC (ex: "14:00")
            TimeSpan intervalEnd = intervalStart.Add(TimeSpan.FromHours(1));

            // Convertim 'now' (UTC) în ora locală a României
            DateTime localNow = TimeZoneInfo.ConvertTimeFromUtc(now, RomaniaTimeZone);

            if (realTimeOnly)
            {
                // Comparație în ora locală, presupunând că intervalStart este ora locală a intervalului
                // Notă: Logica aici devine complexă. Presupunem că 'hourInterval' este ora locală.
                // Logica din Program.cs trimite 'logTimeUtc.ToString("HH:00")'
                // Această întreagă funcție trebuie regândită dacă 'hourInterval' este UTC
                // PENTRU MOMENT: Vom folosi 'localNow' pentru comparațiile de timp
                
                TimeSpan localIntervalStart = intervalStart; // Asumăm 'hourInterval' ca fiind local
                TimeSpan localIntervalEnd = localIntervalStart.Add(TimeSpan.FromHours(1));

                if (localNow.Hour != localIntervalStart.Hours)
                {
                    return 0; 
                }
                double secondsElapsed = localNow.TimeOfDay.TotalSeconds - localIntervalStart.TotalSeconds;
                totalMinutesInInterval = secondsElapsed > 0 ? (secondsElapsed / 60.0) : 0;
            }

            double breakMinutes = 0;
            var breaks = await db.ShiftBreaks.Where(b => b.ShiftId == shiftId).ToListAsync();

            foreach (var b in breaks)
            {
                var breakStart = b.BreakTime; // Local
                var breakEnd = b.BreakTime.Add(TimeSpan.FromMinutes(b.DurationMinutes)); // Local

                var queryStart = intervalStart; // Asumat local
                var queryEnd = realTimeOnly ? localNow.TimeOfDay : intervalEnd; // Asumat local

                if (queryStart < breakEnd && breakStart < queryEnd)
                {
                    var overlapStart = queryStart > breakStart ? queryStart : breakStart;
                    var overlapEnd = queryEnd < breakEnd ? queryEnd : breakEnd;
                    var overlapMinutes = (overlapEnd - overlapStart).TotalMinutes;

                    if (overlapMinutes > 0)
                        breakMinutes += overlapMinutes;
                }
            }

            double dummyMinutes = 0;
            var plannedDowntime = await db.PlannedDowntimes
                .FirstOrDefaultAsync(pd => pd.LineId == lineId && pd.ProductId == productId);

            if (plannedDowntime != null)
            {
                dummyMinutes = plannedDowntime.MinutesPerHour * (totalMinutesInInterval / 60.0);
            }

            double availableMinutes = totalMinutesInInterval - breakMinutes - dummyMinutes;
            if (availableMinutes < 0) availableMinutes = 0;

            return (int)Math.Floor((availableMinutes * 60) / (double)(product.CycleTimeSeconds ?? 1));
        }

        /// <summary>
        /// Găsește sau creează un log de producție pentru intervalul dat.
        /// </summary>
        public static async Task<ProductionLog> GetOrCreateProductionLog(MesDbContext db, int lineId, int productId, int shiftId, string hourInterval, DateTime date)
        {
            var existingLog = await db.ProductionLogs
               .FirstOrDefaultAsync(pl =>
                   pl.LineId == lineId &&
                   pl.HourInterval == hourInterval &&
                   pl.ShiftId == shiftId &&
                   pl.Timestamp.Date == date.Date); // Comparăm .Date

            if (existingLog != null)
            {
                return existingLog;
            }
            else
            {
                var newLog = new ProductionLog
                {
                    LineId = lineId,
                    ProductId = productId,
                    ShiftId = shiftId,
                    HourInterval = hourInterval,
                    Timestamp = date.Date.Add(TimeSpan.Parse(hourInterval)), // Stocat ca UTC
                    ActualParts = 0,
                    ScrapParts = 0,
                    NrftParts = 0,
                    TargetParts = 0,
                    DeclaredDowntimeMinutes = 0,
                    SystemStopMinutes = 0,
                    JustificationRequired = false
                };
                db.ProductionLogs.Add(newLog);
                return newLog;
            }
        }

        /// <summary>
        /// Calculează data corectă a schimbului (gestionează schimburile de noapte).
        /// 'now' ESTE AȘTEPTAT SĂ FIE ORA UTC (DateTime.UtcNow).
        /// </summary>
        public static DateTime GetShiftDate(DateTime now, Shift? currentShift)
        {
            // Convertim ora UTC la ora locală a României PENTRU A PUTEA COMPARA
            DateTime localNow = TimeZoneInfo.ConvertTimeFromUtc(now, RomaniaTimeZone);
            
            DateTime dateToReturn;

            if (currentShift != null && currentShift.StartTime > currentShift.EndTime && localNow.TimeOfDay < currentShift.EndTime)
            {
                // Dacă e schimb de noapte (ex: 22:00-06:00) și ora locală e 05:00 dimineața,
                // data producției este ziua anterioară.
                dateToReturn = localNow.Date.AddDays(-1); // Kind=Unspecified
            }
            else
            {
                // Altfel, data producției este ziua locală curentă.
                dateToReturn = localNow.Date; // Kind=Unspecified
            }

            // ================== CORECȚIE BUG CRITIC ==================
            // Specificăm Kind-ul ca UTC. Acest lucru tratează data (ex: '2025-11-10')
            // ca '2025-11-10 00:00:00 UTC'. Acest lucru satisface cerința Npgsql
            // și permite interogării EF Core să funcționeze corect.
            return DateTime.SpecifyKind(dateToReturn, DateTimeKind.Utc);
            // ================== SFÂRȘIT CORECȚIE ==================
        }

        /// <summary>
        /// Generează lista de intervale orare disponibile pentru logare.
        /// 'now' ESTE AȘTEPTAT SĂ FIE ORA UTC (DateTime.UtcNow).
        /// </summary>
        public static List<TimeSlotDto> GetAvailableTimeSlots(Shift? currentShift, DateTime now)
        {
            var slots = new List<TimeSlotDto>();
            if (currentShift == null) return slots;

            // Convertim ora UTC la ora locală a României PENTRU A PUTEA COMPARA
            DateTime localNow = TimeZoneInfo.ConvertTimeFromUtc(now, RomaniaTimeZone);
            var nowTime = localNow.TimeOfDay;

            var shiftStart = currentShift.StartTime;
            var shiftEnd = currentShift.EndTime;

            if (shiftStart > shiftEnd) // Schimb de noapte
            {
                if (nowTime >= shiftStart) // E 23:00 (ziua 1)
                {
                    for (var ts = shiftStart; ts < TimeSpan.FromHours(24); ts = ts.Add(TimeSpan.FromHours(1)))
                    {
                        slots.Add(new TimeSlotDto(ts, localNow));
                    }
                    for (var ts = TimeSpan.Zero; ts < shiftEnd; ts = ts.Add(TimeSpan.FromHours(1)))
                    {
                        slots.Add(new TimeSlotDto(ts, localNow, 1));
                    }
                }
                else // E 04:00 (ziua 2)
                {
                    for (var ts = shiftStart; ts < TimeSpan.FromHours(24); ts = ts.Add(TimeSpan.FromHours(1)))
                    {
                        slots.Add(new TimeSlotDto(ts, localNow, -1));
                    }
                    for (var ts = TimeSpan.Zero; ts < shiftEnd; ts = ts.Add(TimeSpan.FromHours(1)))
                    {
                        slots.Add(new TimeSlotDto(ts, localNow));
                    }
                }
            }
            else // Schimb de zi
            {
                for (var ts = shiftStart; ts < shiftEnd; ts = ts.Add(TimeSpan.FromHours(1)))
                {
                    slots.Add(new TimeSlotDto(ts, localNow));
                }
            }

            return slots.OrderBy(s => s.Value).ToList();
        }

        /// <summary>
        /// Actualizează log-ul orar cu minutele de oprire sau funcționare.
        /// 'startTime' și 'endTime' SUNT AȘTEPTATE SĂ FIE ORA UTC.
        /// </summary>
        public static async Task UpdateSystemDowntime(MesDbContext db, int lineId, DateTime? startTime, DateTime endTime, bool isRunning)
        {
            if (!startTime.HasValue) return;

            // Asigurăm că orele sunt UTC
            var start = DateTime.SpecifyKind(startTime.Value, DateTimeKind.Utc);
            var end = DateTime.SpecifyKind(endTime, DateTimeKind.Utc);


            while (start < end)
            {
                // MODIFICAT: Asigură-te că ora este tratată ca UTC
                var hourStart = new DateTime(start.Year, start.Month, start.Day, start.Hour, 0, 0, DateTimeKind.Utc);
                var hourEnd = hourStart.AddHours(1);
                var effectiveEnd = (end < hourEnd) ? end : hourEnd;
                var durationInHour = (effectiveEnd - start).TotalMinutes;

                if (durationInHour > 0)
                {
                    var lineStatus = await db.LineStatuses.Include(ls => ls.CurrentShift).FirstOrDefaultAsync(ls => ls.LineId == lineId);
                    if (lineStatus?.CurrentShiftId != null && lineStatus.ProductId.HasValue)
                    {
                        // Aici 'start' este UTC și este trimis la GetShiftDate, care știe să îl gestioneze.
                        var dateToQuery = GetShiftDate(start, lineStatus.CurrentShift);
                        var hourInterval = hourStart.ToString("HH:00"); // Intervalul orar UTC

                        var log = await GetOrCreateProductionLog(db, lineId, lineStatus.ProductId.Value, lineStatus.CurrentShiftId.Value, hourInterval, dateToQuery);

                        if (!isRunning)
                        {
                            log.SystemStopMinutes = (log.SystemStopMinutes ?? 0) + (int)Math.Round(durationInHour);
                        }
                    }
                }
                start = hourEnd;
            }
        }

        /// <summary>
        /// Verifică regulile de "Stop la Defect".
        /// </summary>
        public static async Task<(bool ShouldStop, string Reason)> CheckStopOnDefectRules(
            MesDbContext db, LineStatus lineStatus, ProductionLog currentLog, string userName)
        {
            var rules = await db.StopOnDefectRules.Where(r =>
                r.LineId == lineStatus.LineId &&
                (r.ProductId == lineStatus.ProductId || r.ProductId == null))
                .ToListAsync();

            if (!rules.Any()) return (false, "");

            var rule = rules.FirstOrDefault(r => r.ProductId == lineStatus.ProductId) ?? rules.First();
            
            // ****** MODIFICARE CRITICĂ: Folosim DateTime.UtcNow ******
            var now = DateTime.UtcNow;
            // ****** SFÂRȘIT MODIFICARE ******

            var shift = await db.Shifts.FindAsync(lineStatus.CurrentShiftId);
            var dateToQuery = GetShiftDate(now, shift); // Funcționează corect cu UTC

            var logsThisShift = await db.ProductionLogs
                .Where(pl =>
                    pl.LineId == lineStatus.LineId &&
                    pl.ShiftId == lineStatus.CurrentShiftId &&
                    pl.Timestamp.Date == dateToQuery)
                .OrderBy(pl => pl.HourInterval)
                .ToListAsync();

            if (rule.MaxNrftPerHour > 0 && currentLog.NrftParts > rule.MaxNrftPerHour)
            {
                string reason = $"STOP LA DEFECT: {currentLog.NrftParts} NRFT-uri într-o oră (Limita: {rule.MaxNrftPerHour})";
                await TriggerStopOnDefect(db, lineStatus, reason, userName);
                return (true, reason);
            }

            if (rule.MaxConsecutiveScrap > 0 && currentLog.ScrapParts > 0 && currentLog.NrftParts == 0 && currentLog.ActualParts == 0)
            {
                int consecutiveScrapLogs = 0;
                foreach (var log in logsThisShift.AsEnumerable().Reverse())
                {
                    if (log.ScrapParts > 0 && log.NrftParts == 0 && log.ActualParts == 0) consecutiveScrapLogs++;
                    else break;
                }

                if (consecutiveScrapLogs >= rule.MaxConsecutiveScrap)
                {
                    string reason = $"STOP LA DEFECT: {consecutiveScrapLogs} logări orare consecutive cu Rebut (Limita: {rule.MaxConsecutiveScrap})";
                    await TriggerStopOnDefect(db, lineStatus, reason, userName);
                    return (true, reason);
                }
            }

            if (rule.MaxConsecutiveNRFT > 0 && currentLog.NrftParts > 0 && currentLog.ScrapParts == 0 && currentLog.ActualParts == 0)
            {
                int consecutiveNrftLogs = 0;
                foreach (var log in logsThisShift.AsEnumerable().Reverse())
                {
                    if (log.NrftParts > 0 && log.ScrapParts == 0 && log.ActualParts == 0) consecutiveNrftLogs++;
                    else break;
                }

                if (consecutiveNrftLogs >= rule.MaxConsecutiveNRFT)
                {
                    string reason = $"STOP LA DEFECT: {consecutiveNrftLogs} logări orare consecutive cu NRFT (Limita: {rule.MaxConsecutiveNRFT})";
                    await TriggerStopOnDefect(db, lineStatus, reason, userName);
                    return (true, reason);
                }
            }

            return (false, "");
        }

        /// <summary>
        /// Oprește linia și creează automat un tichet de intervenție.
        /// </summary>
        public static async Task TriggerStopOnDefect(MesDbContext db, LineStatus lineStatus, string reason, string userName)
        {
            lineStatus.Status = "Breakdown";
            await UpdateSystemDowntime(db, lineStatus.LineId, lineStatus.LastStatusChange, DateTime.UtcNow, true); // MODIFICAT: UtcNow
            lineStatus.LastStatusChange = DateTime.UtcNow; // MODIFICAT: UtcNow

            var reasonCode = await db.BreakdownReasons.FirstOrDefaultAsync(br => br.Name == "Stop la Defect");
            if (reasonCode == null)
            {
                reasonCode = new BreakdownReason { Name = "Stop la Defect" };
                db.BreakdownReasons.Add(reasonCode);
                await db.SaveChangesAsync();
            }

            var equipment = await db.Equipments.FirstOrDefaultAsync(e => e.LineId == lineStatus.LineId);
            if (equipment == null)
            {
                await db.SaveChangesAsync();
                return;
            }

            var tichet = new InterventieTichet
            {
                UnicIdTicket = Guid.NewGuid(),
                Status = "Deschis",
                LineId = lineStatus.LineId,
                EquipmentId = equipment.Id,
                DataRaportareOperator = DateTime.UtcNow
            };

            db.InterventieTichete.Add(tichet);
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Funcție helper pentru a reseta starea liniei
        /// </summary>
        public static async Task CheckAndRestoreLineStatus(MesDbContext db, int equipmentId)
        {
            var equipment = await db.Equipments.FindAsync(equipmentId);
            if (equipment == null || !equipment.LineId.HasValue) return;

            var lineId = equipment.LineId.Value;

            var otherActiveTickets = await db.InterventieTichete
                .Include(t => t.Equipment)
                .Where(t => t.Equipment != null && t.Equipment.LineId == lineId &&
                            t.Status != "Finalizat" &&
                            t.Status != "Anulat")
                .AnyAsync();

            if (!otherActiveTickets)
            {
                var lineStatus = await db.LineStatuses.FirstOrDefaultAsync(ls => ls.LineId == lineId);

                if (lineStatus != null && lineStatus.Status == "Breakdown")
                {
                    lineStatus.Status = "Stopped";
                    await UpdateSystemDowntime(db, lineId, lineStatus.LastStatusChange ?? DateTime.UtcNow, DateTime.UtcNow, false); // MODIFICAT: UtcNow
                    lineStatus.LastStatusChange = DateTime.UtcNow; // MODIFICAT: UtcNow

                    await db.SaveChangesAsync();
                }
            }
        }
    }
}