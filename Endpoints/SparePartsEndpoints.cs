using MesEnterprise.Data;
using MesEnterprise.Models.Inventory;
using MesEnterprise.Models.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace MesEnterprise.Endpoints
{
    public static class SparePartsEndpoints
    {
        public static IEndpointRouteBuilder MapSparePartsApi(this IEndpointRouteBuilder app)
        {
            var sparePartsApi = app.MapGroup("/api/spare-parts");

            // GET /api/spare-parts - List all spare parts
            sparePartsApi.MapGet("/", async (MesDbContext db) =>
            {
                var parts = await db.SpareParts
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                return Results.Ok(parts);
            });

            // GET /api/spare-parts/{id} - Get specific spare part
            sparePartsApi.MapGet("/{id}", async (int id, MesDbContext db) =>
            {
                var part = await db.SpareParts.FindAsync(id);
                
                if (part == null)
                    return Results.NotFound(new { Message = "Piesa nu a fost găsită" });

                return Results.Ok(part);
            });

            // POST /api/spare-parts - Create new spare part
            sparePartsApi.MapPost("/", async ([FromBody] SparePart part, MesDbContext db) =>
            {
                db.SpareParts.Add(part);
                await db.SaveChangesAsync();

                return Results.Created($"/api/spare-parts/{part.Id}", part);
            });

            // PUT /api/spare-parts/{id} - Update spare part
            sparePartsApi.MapPut("/{id}", async (int id, [FromBody] SparePart updatedPart, MesDbContext db) =>
            {
                var part = await db.SpareParts.FindAsync(id);
                
                if (part == null)
                    return Results.NotFound(new { Message = "Piesa nu a fost găsită" });

                part.PartNumber = updatedPart.PartNumber;
                part.Name = updatedPart.Name;
                part.Description = updatedPart.Description;
                part.Location = updatedPart.Location;
                part.QuantityInStock = updatedPart.QuantityInStock;
                part.MinimumStock = updatedPart.MinimumStock;
                part.UnitCost = updatedPart.UnitCost;
                part.IsActive = updatedPart.IsActive;

                await db.SaveChangesAsync();

                return Results.Ok(part);
            });

            // DELETE /api/spare-parts/{id} - Deactivate spare part
            sparePartsApi.MapDelete("/{id}", async (int id, MesDbContext db) =>
            {
                var part = await db.SpareParts.FindAsync(id);
                
                if (part == null)
                    return Results.NotFound(new { Message = "Piesa nu a fost găsită" });

                part.IsActive = false;
                await db.SaveChangesAsync();

                return Results.Ok(new { Message = "Piesa a fost dezactivată" });
            });

            // GET /api/spare-parts/statistics - Get inventory statistics
            sparePartsApi.MapGet("/statistics", async (MesDbContext db) =>
            {
                var parts = await db.SpareParts
                    .Where(p => p.IsActive)
                    .ToListAsync();

                var stats = new
                {
                    TotalParts = parts.Count,
                    LowStockParts = parts.Count(p => p.QuantityInStock <= p.MinimumStock),
                    InventoryValue = parts.Sum(p => p.QuantityInStock * p.UnitCost),
                    PendingOrders = 0 // TODO: Implement orders tracking
                };

                return Results.Ok(stats);
            });

            // GET /api/spare-parts/{id}/usage-history - Get usage history for a part
            sparePartsApi.MapGet("/{id}/usage-history", async (int id, MesDbContext db) =>
            {
                var usages = await db.SparePartUsages
                    .Where(u => u.SparePartId == id)
                    .Include(u => u.InterventieTichet)
                    .OrderByDescending(u => u.UsedAt)
                    .Select(u => new
                    {
                        u.Id,
                        u.UsedAt,
                        InterventionId = u.InterventieTichetId,
                        u.QuantityUsed,
                        UserName = u.UsedByUserId.HasValue ? "User" : null, // TODO: Add user lookup
                        u.Notes
                    })
                    .ToListAsync();

                return Results.Ok(usages);
            });

            // POST /api/spare-parts/use - Record spare part usage in intervention
            sparePartsApi.MapPost("/use", async ([FromBody] SparePartUsageRequest request, MesDbContext db) =>
            {
                var part = await db.SpareParts.FindAsync(request.SparePartId);
                
                if (part == null)
                    return Results.NotFound(new { Message = "Piesa nu a fost găsită" });

                if (part.QuantityInStock < request.QuantityUsed)
                    return Results.BadRequest(new { Message = "Stoc insuficient" });

                // Create usage record
                var usage = new SparePartUsage
                {
                    SparePartId = request.SparePartId,
                    InterventieTichetId = request.InterventieTichetId,
                    QuantityUsed = request.QuantityUsed,
                    UsedAt = DateTime.UtcNow,
                    UsedByUserId = request.UsedByUserId,
                    Notes = request.Notes
                };

                db.SparePartUsages.Add(usage);

                // Update stock
                part.QuantityInStock -= request.QuantityUsed;

                await db.SaveChangesAsync();

                return Results.Ok(new { Message = "Utilizare înregistrată cu succes", Usage = usage });
            });

            // GET /api/spare-parts/low-stock - Get parts with low stock
            sparePartsApi.MapGet("/low-stock", async (MesDbContext db) =>
            {
                var parts = await db.SpareParts
                    .Where(p => p.IsActive && p.QuantityInStock <= p.MinimumStock)
                    .OrderBy(p => p.QuantityInStock)
                    .ToListAsync();

                return Results.Ok(parts);
            });

            return app;
        }
    }

    public record SparePartUsageRequest(
        int SparePartId,
        int InterventieTichetId,
        int QuantityUsed,
        int? UsedByUserId,
        string? Notes
    );
}
