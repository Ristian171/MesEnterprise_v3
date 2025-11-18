using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Alerts
{
    public class AlertRule
    {
        public int Id { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string RuleType { get; set; } = string.Empty;
        public int? LineId { get; set; }
        public int ThresholdValue { get; set; }
        public string Severity { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? RuleConfiguration { get; set; }
        public DateTime CreatedAt { get; set; }
        public int EvaluationWindowMinutes { get; set; }

        // Navigation properties
        public Line? Line { get; set; }
    }
}
