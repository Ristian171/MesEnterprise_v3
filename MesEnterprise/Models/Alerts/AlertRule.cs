using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Core;
using MesEnterprise.Models.Alerts;

namespace MesEnterprise.Models.Alerts
{
    public class AlertRule
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public int EvaluationWindowMinutes { get; set; }

        public bool IsActive { get; set; }

        public int? LineId { get; set; }

        public string? RuleConfiguration { get; set; }

        [MaxLength(200)]
        public string? RuleName { get; set; }

        [MaxLength(50)]
        public string? RuleType { get; set; }

        [MaxLength(50)]
        public string? Severity { get; set; }

        public int ThresholdValue { get; set; }

        // Navigation properties
        public Line? Line { get; set; }
        public ICollection<AlertLog> AlertLogs { get; set; } = new List<AlertLog>();
    }
}