using System.ComponentModel.DataAnnotations;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Maintenance
{
    public class PreventiveMaintenancePlan
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int? LineId { get; set; }
        public Line? Line { get; set; }

        public int? EquipmentId { get; set; }
        public Equipment? Equipment { get; set; }

        [MaxLength(50)]
        public string FrequencyType { get; set; } = "Days";

        public int FrequencyValue { get; set; }

        public DateTime? LastExecutedDate { get; set; }

        public DateTime? NextDueDate { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Checklist { get; set; }
    }
}
