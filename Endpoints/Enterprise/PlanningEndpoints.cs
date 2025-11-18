using MesEnterprise.Data;
using MesEnterprise.Models.Planning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MesEnterprise.Endpoints.Enterprise;

public static class PlanningEndpoints
{
    public static IEndpointRouteBuilder MapPlanningEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/planning")
            .WithTags("Planning")
            .RequireAuthorization("OperatorOrHigher");

        // Get work orders
        group.MapGet("/workorders", async (MesDbContext db, string? status, int? lineId) =>
        {
            var query = db.ProductionWorkOrders.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(wo => wo.Status == status);
            }

            if (lineId.HasValue)
            {
                query = query.Where(wo => wo.LineId == lineId.Value);
            }

            var workOrders = await query
                .Include(wo => wo.Line)
                .Include(wo => wo.Product)
                .OrderByDescending(wo => wo.CreatedAt)
                .ToListAsync();

            return Results.Ok(workOrders);
        });

        // Get work order by ID
        group.MapGet("/workorders/{id:int}", async (MesDbContext db, int id) =>
        {
            var workOrder = await db.ProductionWorkOrders
                .Include(wo => wo.Line)
                .Include(wo => wo.Product)
                .FirstOrDefaultAsync(wo => wo.Id == id);

            return workOrder == null ? Results.NotFound() : Results.Ok(workOrder);
        });

        // Create work order
        group.MapPost("/workorders", [Authorize(Policy = "AdminOnly")] async (MesDbContext db, ProductionWorkOrder workOrder) =>
        {
            workOrder.CreatedAt = DateTime.UtcNow;
            workOrder.Status = "Pending";
            workOrder.ProducedQuantity = 0;

            db.ProductionWorkOrders.Add(workOrder);
            await db.SaveChangesAsync();

            return Results.Created($"/api/planning/workorders/{workOrder.Id}", workOrder);
        });

        // Update work order
        group.MapPut("/workorders/{id:int}", [Authorize(Policy = "AdminOnly")] async (MesDbContext db, int id, ProductionWorkOrder updatedWorkOrder) =>
        {
            var workOrder = await db.ProductionWorkOrders.FindAsync(id);
            if (workOrder == null)
            {
                return Results.NotFound();
            }

            workOrder.WorkOrderNumber = updatedWorkOrder.WorkOrderNumber;
            workOrder.ProductId = updatedWorkOrder.ProductId;
            workOrder.LineId = updatedWorkOrder.LineId;
            workOrder.PlannedQuantity = updatedWorkOrder.PlannedQuantity;
            workOrder.PlannedStartDate = updatedWorkOrder.PlannedStartDate;
            workOrder.PlannedEndDate = updatedWorkOrder.PlannedEndDate;
            workOrder.Status = updatedWorkOrder.Status;
            workOrder.Notes = updatedWorkOrder.Notes;

            await db.SaveChangesAsync();

            return Results.Ok(workOrder);
        });

        // Delete work order
        group.MapDelete("/workorders/{id:int}", [Authorize(Policy = "AdminOnly")] async (MesDbContext db, int id) =>
        {
            var workOrder = await db.ProductionWorkOrders.FindAsync(id);
            if (workOrder == null)
            {
                return Results.NotFound();
            }

            db.ProductionWorkOrders.Remove(workOrder);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        // Get active work order for line
        group.MapGet("/workorders/active/{lineId:int}", async (MesDbContext db, int lineId) =>
        {
            var activeWorkOrder = await db.ProductionWorkOrders
                .Where(wo => wo.LineId == lineId && wo.Status == "InProgress")
                .Include(wo => wo.Product)
                .FirstOrDefaultAsync();

            return activeWorkOrder == null ? Results.NotFound() : Results.Ok(activeWorkOrder);
        });

        return app;
    }
}
