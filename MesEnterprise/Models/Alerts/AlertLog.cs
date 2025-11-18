using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Alerts;

namespace MesEnterprise.Models.Alerts
{
    public class AlertLog
    {
        [Key]
        public int Id { get; set; }

        public DateTime? AcknowledgedAt { get; set; }

        public int? AlertRuleId { get; set; }

        public bool IsAcknowledged { get; set; }

        public string? Message { get; set; }

        [MaxLength(50)]
        public string? Severity { get; set; }

        public DateTime TriggeredAt { get; set; }

        // Navigation properties
        public AlertRule? AlertRule { get; set; }
    }
}