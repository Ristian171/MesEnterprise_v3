using MesEnterprise.Data;
using MesEnterprise.Models.Core;
using MesEnterprise.Models.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MesEnterprise.Services
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MesDbContext>();
            var passwordService = scope.ServiceProvider.GetRequiredService<PasswordService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // Apply migrations
                await db.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully");

                // Seed default roles
                if (!await db.Roles.AnyAsync())
                {
                    var roles = new[]
                    {
                        new Role { Name = "Admin", Description = "Administrator with full access" },
                        new Role { Name = "Operator", Description = "Production operator" },
                        new Role { Name = "InginerMentenanta", Description = "Maintenance engineer" },
                        new Role { Name = "PlantManager", Description = "Plant manager" },
                        new Role { Name = "TeamLeader", Description = "Team leader" },
                        new Role { Name = "Quality", Description = "Quality inspector" },
                        new Role { Name = "Warehouse", Description = "Warehouse operator" },
                        new Role { Name = "Planner", Description = "Production planner" }
                    };
                    
                    db.Roles.AddRange(roles);
                    await db.SaveChangesAsync();
                    logger.LogInformation("Default roles created");
                }

                // Seed admin user
                if (!await db.Users.AnyAsync(u => u.Username == "admin"))
                {
                    var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                    var adminUser = new User
                    {
                        Username = "admin",
                        PasswordHash = passwordService.HashPassword("admin"),
                        RoleId = adminRole?.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    db.Users.Add(adminUser);
                    await db.SaveChangesAsync();
                    logger.LogInformation("Admin user created (username: admin, password: admin)");
                }

                // Seed default shifts
                if (!await db.Shifts.AnyAsync())
                {
                    var shifts = new[]
                    {
                        new Shift { Name = "Shift 1", StartTime = new TimeSpan(6, 0, 0), EndTime = new TimeSpan(14, 0, 0) },
                        new Shift { Name = "Shift 2", StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(22, 0, 0) },
                        new Shift { Name = "Shift 3", StartTime = new TimeSpan(22, 0, 0), EndTime = new TimeSpan(6, 0, 0) }
                    };
                    
                    db.Shifts.AddRange(shifts);
                    await db.SaveChangesAsync();
                    logger.LogInformation("Default shifts created");
                }

                // Seed system settings with module toggles
                if (!await db.SystemSettings.AnyAsync())
                {
                    var settings = new[]
                    {
                        new SystemSetting { Key = "Production_Module_Enabled", Value = "true", Description = "Enable Production module" },
                        new SystemSetting { Key = "PM_Module_Enabled", Value = "true", Description = "Enable Preventive Maintenance module" },
                        new SystemSetting { Key = "TPM_Module_Enabled", Value = "false", Description = "Enable TPM module" },
                        new SystemSetting { Key = "Quality_Module_Enabled", Value = "true", Description = "Enable Quality module" },
                        new SystemSetting { Key = "Inventory_Module_Enabled", Value = "false", Description = "Enable Inventory module" },
                        new SystemSetting { Key = "Analysis_Module_Enabled", Value = "true", Description = "Enable Analysis module" },
                        new SystemSetting { Key = "Alerts_Module_Enabled", Value = "true", Description = "Enable Alerts module" },
                        new SystemSetting { Key = "Export_Module_Enabled", Value = "true", Description = "Enable Export module" },
                        new SystemSetting { Key = "LiveView_Module_Enabled", Value = "false", Description = "Enable Live View module" },
                        new SystemSetting { Key = "Scan_Mode_Enabled", Value = "false", Description = "Enable scan mode" },
                        new SystemSetting { Key = "Good_Parts_Logging_Mode", Value = "Overwrite", Description = "Overwrite or Aggregate" },
                        new SystemSetting { Key = "Downtime_Scrap_Logging_Mode", Value = "Overwrite", Description = "Overwrite or Aggregate" },
                        new SystemSetting { Key = "Justification_Threshold_Percent", Value = "80", Description = "OEE threshold for justification requirement" },
                        new SystemSetting { Key = "Require_Justification", Value = "true", Description = "Require justification for low OEE" }
                    };
                    
                    db.SystemSettings.AddRange(settings);
                    await db.SaveChangesAsync();
                    logger.LogInformation("Default system settings created");
                }

                logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during database initialization");
                throw;
            }
        }
    }
}
