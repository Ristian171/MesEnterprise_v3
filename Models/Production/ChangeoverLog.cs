using System.ComponentModel.DataAnnotations;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Production
{
    public class ChangeoverLog
    {
        public int Id { get; set; }

        public int LineId { get; set; }

        public int? ProductFromId { get; set; }

        public int? FromProductId { get; set; }

        public int ProductToId { get; set; }

        public int ToProductId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int? ActualDurationMinutes { get; set; }

        public int? PlannedDurationMinutes { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public int? PerformedByUserId { get; set; }

        // Navigation properties
        public Line Line { get; set; } = null!;
        public Product? FromProduct { get; set; }
        public Product? ProductFrom { get; set; }
        public Product ToProduct { get; set; } = null!;
        public Product ProductTo { get; set; } = null!;
        public User? PerformedByUser { get; set; }
    }
}
