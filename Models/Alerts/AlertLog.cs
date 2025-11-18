namespace MesEnterprise.Models.Alerts
{
    public class AlertLog
    {
        public int Id { get; set; }
        public int? AlertRuleId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime TriggeredAt { get; set; }
        public bool IsAcknowledged { get; set; }
        public DateTime? AcknowledgedAt { get; set; }

        // Navigation properties
        public AlertRule? AlertRule { get; set; }
    }
}
