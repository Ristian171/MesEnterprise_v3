using MesEnterprise.Data;
using MesEnterprise.Models.Maintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace MesEnterprise.Endpoints
{
    public static class TPMEndpoints
    {
        public static IEndpointRouteBuilder MapTPMApi(this IEndpointRouteBuilder app)
        {
            var tpmApi = app.MapGroup("/api/tpm");

            // GET /api/tpm/statistics
            tpmApi.MapGet("/statistics", async (MesDbContext db) =>
            {
                var activePlans = await db.PreventiveMaintenancePlans
                    .Where(p => p.IsActive)
                    .CountAsync();

                var today = DateTime.UtcNow.Date;
                var todayTasks = await db.PreventiveMaintenancePlans
                    .Where(p => p.IsActive && p.NextDueDate.HasValue && p.NextDueDate.Value.Date == today)
                    .CountAsync();

                var overdueTasks = await db.PreventiveMaintenancePlans
                    .Where(p => p.IsActive && p.NextDueDate.HasValue && p.NextDueDate.Value < DateTime.UtcNow)
                    .CountAsync();

                var startOfMonth = new DateTime(today.Year, today.Month, 1);
                var completedMonth = await db.PreventiveMaintenancePlans
                    .Where(p => p.LastExecutedDate.HasValue && p.LastExecutedDate.Value >= startOfMonth)
                    .CountAsync();

                return Results.Ok(new
                {
                    ActivePlans = activePlans,
                    TodayTasks = todayTasks,
                    OverdueTasks = overdueTasks,
                    CompletedMonth = completedMonth
                });
            });

            // GET /api/tpm/plans
            tpmApi.MapGet("/plans", async (MesDbContext db) =>
            {
                var plans = await db.PreventiveMaintenancePlans
                    .Include(p => p.Line)
                    .Include(p => p.Equipment)
                    .OrderBy(p => p.Name)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Description,
                        p.LineId,
                        LineName = p.Line != null ? p.Line.Name : null,
                        p.EquipmentId,
                        EquipmentName = p.Equipment != null ? p.Equipment.Name : null,
                        p.FrequencyType,
                        p.FrequencyValue,
                        p.LastExecutedDate,
                        p.NextDueDate,
                        p.IsActive,
                        p.Checklist
                    })
                    .ToListAsync();

                return Results.Ok(plans);
            });

            // GET /api/tpm/plans/{id}
            tpmApi.MapGet("/plans/{id}", async (int id, MesDbContext db) =>
            {
                var plan = await db.PreventiveMaintenancePlans.FindAsync(id);
                
                if (plan == null)
                    return Results.NotFound(new { Message = "Plan not found" });

                return Results.Ok(plan);
            });

            // POST /api/tpm/plans
            tpmApi.MapPost("/plans", async ([FromBody] PreventiveMaintenancePlan plan, MesDbContext db) =>
            {
                // Calculate next due date based on frequency
                plan.NextDueDate = CalculateNextDueDate(DateTime.UtcNow, plan.FrequencyType, plan.FrequencyValue);
                
                db.PreventiveMaintenancePlans.Add(plan);
                await db.SaveChangesAsync();

                return Results.Created($"/api/tpm/plans/{plan.Id}", plan);
            });

            // PUT /api/tpm/plans/{id}
            tpmApi.MapPut("/plans/{id}", async (int id, [FromBody] PreventiveMaintenancePlan updatedPlan, MesDbContext db) =>
            {
                var plan = await db.PreventiveMaintenancePlans.FindAsync(id);
                
                if (plan == null)
                    return Results.NotFound(new { Message = "Plan not found" });

                plan.Name = updatedPlan.Name;
                plan.Description = updatedPlan.Description;
                plan.LineId = updatedPlan.LineId;
                plan.EquipmentId = updatedPlan.EquipmentId;
                plan.FrequencyType = updatedPlan.FrequencyType;
                plan.FrequencyValue = updatedPlan.FrequencyValue;
                plan.Checklist = updatedPlan.Checklist;
                plan.IsActive = updatedPlan.IsActive;

                // Recalculate next due date if frequency changed
                if (plan.LastExecutedDate.HasValue)
                {
                    plan.NextDueDate = CalculateNextDueDate(plan.LastExecutedDate.Value, plan.FrequencyType, plan.FrequencyValue);
                }

                await db.SaveChangesAsync();

                return Results.Ok(plan);
            });

            // DELETE /api/tpm/plans/{id}
            tpmApi.MapDelete("/plans/{id}", async (int id, MesDbContext db) =>
            {
                var plan = await db.PreventiveMaintenancePlans.FindAsync(id);
                
                if (plan == null)
                    return Results.NotFound(new { Message = "Plan not found" });

                plan.IsActive = false;
                await db.SaveChangesAsync();

                return Results.Ok(new { Message = "Plan deactivated" });
            });

            // POST /api/tpm/execute/{id}
            tpmApi.MapPost("/execute/{id}", async (int id, [FromBody] ExecutionRecord execution, MesDbContext db) =>
            {
                var plan = await db.PreventiveMaintenancePlans.FindAsync(id);
                
                if (plan == null)
                    return Results.NotFound(new { Message = "Plan not found" });

                // Update execution dates
                plan.LastExecutedDate = DateTime.UtcNow;
                plan.NextDueDate = CalculateNextDueDate(plan.LastExecutedDate.Value, plan.FrequencyType, plan.FrequencyValue);

                await db.SaveChangesAsync();

                return Results.Ok(new { Message = "Execution recorded successfully", NextDueDate = plan.NextDueDate });
            });

            return app;
        }

        private static DateTime CalculateNextDueDate(DateTime from, string frequencyType, int frequencyValue)
        {
            return frequencyType switch
            {
                "Days" => from.AddDays(frequencyValue),
                "Weeks" => from.AddDays(frequencyValue * 7),
                "Months" => from.AddMonths(frequencyValue),
                "Years" => from.AddYears(frequencyValue),
                _ => from.AddDays(frequencyValue)
            };
        }
    }

    public record ExecutionRecord(DateTime ExecutedAt, string? Notes);
}
