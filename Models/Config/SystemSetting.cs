using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Config
{
    public class SystemSetting
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Value { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Module toggle constants
        public const string ProductionModuleEnabled = "Production_Module_Enabled";
        public const string PmModuleEnabled = "PM_Module_Enabled";
        public const string TpmModuleEnabled = "TPM_Module_Enabled";
        public const string QualityModuleEnabled = "Quality_Module_Enabled";
        public const string InventoryModuleEnabled = "Inventory_Module_Enabled";
        public const string AnalysisModuleEnabled = "Analysis_Module_Enabled";
        public const string AlertsModuleEnabled = "Alerts_Module_Enabled";
        public const string ExportModuleEnabled = "Export_Module_Enabled";
        public const string LiveViewModuleEnabled = "LiveView_Module_Enabled";

        // Operational settings
        public const string GoodPartsLoggingMode = "GoodParts_LoggingMode";
        public const string DowntimeScrapLoggingMode = "DowntimeScrap_LoggingMode";
        public const string JustificationThresholdPercent = "Justification_Threshold_Percent";
        public const string RequireJustification = "Require_Justification";
        public const string AutoBackupEnabled = "AutoBackup_Enabled";
        public const string AutoBackupIntervalHours = "AutoBackup_Interval_Hours";
    }
}
