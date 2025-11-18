namespace MesEnterprise.Models.Config
{
    public static class SystemSettingKeys
    {
        // Module toggles
        public const string ProductionModuleEnabled = "Production_Module_Enabled";
        public const string PMModuleEnabled = "PM_Module_Enabled";
        public const string TPMModuleEnabled = "TPM_Module_Enabled";
        public const string QualityModuleEnabled = "Quality_Module_Enabled";
        public const string InventoryModuleEnabled = "Inventory_Module_Enabled";
        public const string AnalysisModuleEnabled = "Analysis_Module_Enabled";
        public const string AlertsModuleEnabled = "Alerts_Module_Enabled";
        public const string ExportModuleEnabled = "Export_Module_Enabled";
        public const string LiveViewModuleEnabled = "LiveView_Module_Enabled";
        public const string ScanModeEnabled = "Scan_Mode_Enabled";

        // Logging modes
        public const string GoodPartsLoggingMode = "Good_Parts_Logging_Mode";
        public const string DowntimeScrapLoggingMode = "Downtime_Scrap_Logging_Mode";

        // Justification settings
        public const string JustificationThresholdPercent = "Justification_Threshold_Percent";
        public const string RequireJustification = "Require_Justification";
    }
}
