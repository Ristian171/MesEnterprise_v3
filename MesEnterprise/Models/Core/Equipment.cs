using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Core;
using MesEnterprise.Models.Maintenance;

namespace MesEnterprise.Models.Core
{
    public class Equipment
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string? Code { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime? LastMaintenanceDate { get; set; }

        public int LineId { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        public decimal OreFunctionare { get; set; }

        // Navigation properties
        public Line? Line { get; set; }
        public ICollection<PreventiveMaintenancePlan> MaintenancePlans { get; set; } = new List<PreventiveMaintenancePlan>();
    }
}