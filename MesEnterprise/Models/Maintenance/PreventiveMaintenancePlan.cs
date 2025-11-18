using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Maintenance
{
    public class PreventiveMaintenancePlan
    {
        [Key]
        public int Id { get; set; }

        public int? EquipmentId { get; set; }

        [MaxLength(50)]
        public string? Frequency { get; set; }

        public int FrequencyValue { get; set; }

        public bool IsActive { get; set; }

        public DateTime? LastExecutedDate { get; set; }

        public int? LineId { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        public DateTime? NextDueDate { get; set; }

        public string? TaskDescription { get; set; }

        // Navigation properties
        public Equipment? Equipment { get; set; }
    }
}