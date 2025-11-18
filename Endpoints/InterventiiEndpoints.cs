/*
 * ===================================================================================
 * FIȘIER: MesSimplu/InterventiiApi.cs
 * ROL: API pentru funcțiile de mentenanță/intervenții.
 * STARE: MODIFICAT (IMPLEMENTAT ENDPOINT-URI LIPSĂ)
 *
 * MODIFICARE (Senior Dev):
 * - BUG CRITIC (404): Fișierul era un placeholder.
 * - IMPLEMENTAT GET /api/interventii/deschise: Adăugat endpoint-ul pe
 * care îl caută 'interventii.js'.
 * - IMPLEMENTAT GET /api/interventii/{id}: Adăugat endpoint-ul pentru
 * încărcarea detaliilor.
 * - IMPLEMENTAT PUT /api/interventii/{id}: Adăugat endpoint-ul pentru
 * salvarea intervenției.
 * ===================================================================================
 */

using MesEnterprise.Data;
using MesEnterprise.Models.Core;
using MesEnterprise.Models.Maintenance;
using MesEnterprise.Models.Quality;
using MesEnterprise.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MesEnterprise.Endpoints
{
    public static class InterventiiEndpoints
    {
        public static IEndpointRouteBuilder MapInterventiiApi(this IEndpointRouteBuilder app)
        {
            // MODIFICAT: Securizat pentru Tehnicieni sau Admini
            var interventiiApi = app.MapGroup("/api/interventii").RequireAuthorization("TechOrAdmin");

            // --- ENDPOINT NOU: Rezolvă eroarea 404 ---
            // GET /api/interventii/deschise
            interventiiApi.MapGet("/deschise", async (
                MesDbContext db,
                [FromQuery] int? linieId,
                [FromQuery] int? produsId,
                [FromQuery] int? equipmentId,
                [FromQuery] DateTime? dataStart) =>
            {
                var query = db.InterventieTichete
                    .Where(t => t.Status == "Deschis" || t.Status == "InLucru")
                    .Include(t => t.Line)
                    .Include(t => t.Equipment)
                    .OrderBy(t => t.DataRaportareOperator)
                    .AsQueryable();

                if (linieId.HasValue)
                {
                    query = query.Where(t => t.LineId == linieId.Value);
                }
                if (produsId.HasValue)
                {
                    query = query.Where(t => t.ProductId == produsId.Value);
                }
                if (equipmentId.HasValue)
                {
                    query = query.Where(t => t.EquipmentId == equipmentId.Value);
                }
                if (dataStart.HasValue)
                {
                    var dateOnly = dataStart.Value.Date;
                    query = query.Where(t => t.DataRaportareOperator.Date == dateOnly);
                }

                var tichete = await query.Select(t => new
                {
                    t.Id,
                    t.UnicIdTicket,
                    t.DataRaportareOperator, // Trimis ca UTC
                    t.OperatorNume,
                    Linie = t.Line.Name,
                    Echipament = t.Equipment.Name
                }).ToListAsync();

                return Results.Ok(tichete);
            });

            // --- ENDPOINT NOU: Necesar pentru încărcarea detaliilor ---
            // GET /api/interventii/{id}
            interventiiApi.MapGet("/{id}", async (int id, MesDbContext db) =>
            {
                var tichet = await db.InterventieTichete
                    .Include(t => t.Line)
                    .Include(t => t.Equipment)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (tichet == null)
                {
                    return Results.NotFound(new { Message = "Tichetul nu a fost găsit." });
                }

                // Frontend-ul se așteaptă la un format specific
                return Results.Ok(new
                {
                    tichet.Id,
                    tichet.UnicIdTicket,
                    Line = new { Name = tichet.Line.Name },
                    Equipment = new { Name = tichet.Equipment.Name },
                    tichet.OperatorNume,
                    tichet.DataRaportareOperator,
                    tichet.DataStartInterventie,
                    tichet.DataStopInterventie,
                    tichet.ProblemaRaportataId,
                    tichet.DefectiuneIdentificataId,
                    tichet.DefectiuneTextLiber,
                    tichet.InfluenteazaProdusul,
                    tichet.Status
                });
            });

            // --- ENDPOINT NOU: Necesar pentru salvarea formularului ---
            // PUT /api/interventii/{id}
            interventiiApi.MapPut("/{id}", async (
                int id,
                [FromBody] UpdateInterventieTichetRequest req,
                MesDbContext db,
                ClaimsPrincipal user) =>
            {
                var tichet = await db.InterventieTichete.FindAsync(id);
                if (tichet == null)
                {
                    return Results.NotFound(new { Message = "Tichetul nu a fost găsit." });
                }

                var username = user.FindFirst(ClaimTypes.Name)?.Value;

                // Mapare DTO la Entitate
                tichet.TehnicianNume = username;
                
                // Conversie oră: Clientul trimite ora locală. O convertim în UTC.
                if (req.DataStartInterventie.HasValue)
                    tichet.DataStartInterventie = req.DataStartInterventie.Value.ToUniversalTime();
                
                if (req.DataStopInterventie.HasValue)
                    tichet.DataStopInterventie = req.DataStopInterventie.Value.ToUniversalTime();
                
                tichet.ProblemaRaportataId = req.ProblemaRaportataId;
                tichet.DefectiuneIdentificataId = req.DefectiuneIdentificataId;
                tichet.DefectiuneTextLiber = req.DefectiuneTextLiber;
                tichet.InfluenteazaProdusul = req.InfluenteazaProdusul;

                // Update Status
                if (!string.IsNullOrEmpty(req.Status))
                {
                    tichet.Status = req.Status;
                }
                else
                {
                    return Results.BadRequest(new { Message = "Status invalid." });
                }

                await db.SaveChangesAsync();

                // Dacă tichetul a fost Rezolvat, verificăm dacă linia trebuie repornită
                if (tichet.Status == "Finalizat")
                {
                    await ApiHelpers.CheckAndRestoreLineStatus(db, tichet.EquipmentId);
                }

                return Results.Ok(new { Message = "Intervenție salvată." });
            });

            return app;
        }
    }
}