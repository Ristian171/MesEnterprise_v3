using System.ComponentModel.DataAnnotations;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Alerts
{
    public class AlertRule
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string RuleName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string RuleType { get; set; } = string.Empty;

        public int? LineId { get; set; }
        public Line? Line { get; set; }

        public int ThresholdValue { get; set; }

        [Required, MaxLength(50)]
        public string Severity { get; set; } = "Warning";

        public bool IsActive { get; set; } = true;

        public string? RuleConfiguration { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int EvaluationWindowMinutes { get; set; } = 60;
    }
}
