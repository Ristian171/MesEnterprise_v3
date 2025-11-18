using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Core
{
    public class PlannedDowntime
    {
        public int Id { get; set; }

        public int LineId { get; set; }
        public Line? Line { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int PlannedDowntimeSeconds { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
