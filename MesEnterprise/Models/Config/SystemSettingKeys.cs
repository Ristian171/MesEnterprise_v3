namespace MesEnterprise.Models.Config
{
    /// <summary>
    /// Constants for system setting keys used throughout the application.
    /// </summary>
    public static class SystemSettingKeys
    {
        public const string RequireJustification = "RequireJustification";
        public const string JustificationThresholdPercent = "JustificationThresholdPercent";
        public const string MaxConsecutiveDowntimeMinutes = "MaxConsecutiveDowntimeMinutes";
        public const string AlertEmailRecipients = "AlertEmailRecipients";
        public const string AutoBackupEnabled = "AutoBackupEnabled";
        public const string AutoBackupPath = "AutoBackupPath";
        public const string AutoBackupIntervalHours = "AutoBackupIntervalHours";
    }
}
