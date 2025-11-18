using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Alerts
{
    public class AlertLog
    {
        public int Id { get; set; }

        public int? AlertRuleId { get; set; }
        public AlertRule? AlertRule { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Severity { get; set; } = "Warning";

        public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;

        public bool IsAcknowledged { get; set; }

        public DateTime? AcknowledgedAt { get; set; }
    }
}
