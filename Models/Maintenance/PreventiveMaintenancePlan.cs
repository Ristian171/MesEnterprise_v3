using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Maintenance
{
    public class PreventiveMaintenancePlan
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? TaskDescription { get; set; }
        public int? LineId { get; set; }
        public int? EquipmentId { get; set; }
        public string Frequency { get; set; } = string.Empty;
        public int FrequencyValue { get; set; }
        public DateTime? LastExecutedDate { get; set; }
        public DateTime? NextDueDate { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public Line? Line { get; set; }
        public Equipment? Equipment { get; set; }
    }
}
