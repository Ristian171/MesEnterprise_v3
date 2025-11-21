using System.ComponentModel.DataAnnotations;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Maintenance
{
    public class PreventiveMaintenancePlan
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public int? EquipmentId { get; set; }

        public int? LineId { get; set; }

        [MaxLength(50)]
        public string FrequencyType { get; set; } = "Days";

        public int FrequencyValue { get; set; }

        public DateTime? LastPerformedDate { get; set; }

        public DateTime? NextDueDate { get; set; }

        [MaxLength(2000)]
        public string? TaskDescription { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Equipment? Equipment { get; set; }
        public Line? Line { get; set; }
    }
}
