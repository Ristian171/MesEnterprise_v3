using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Config
{
    public class SystemSetting
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Value { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Module toggle constants
        public const string ModuleProductionTracking = "Module.ProductionTracking";
        public const string ModuleQualityManagement = "Module.QualityManagement";
        public const string ModuleMaintenanceManagement = "Module.MaintenanceManagement";
        public const string ModuleInventoryManagement = "Module.InventoryManagement";
        public const string ModulePlanningScheduling = "Module.PlanningScheduling";
        public const string ModuleAdvancedAnalytics = "Module.AdvancedAnalytics";
        public const string ModuleAlertSystem = "Module.AlertSystem";
    }
}
