using System.ComponentModel.DataAnnotations;
using MesEnterprise.Models.Core;
using MesEnterprise.Models.Planning;

namespace MesEnterprise.Models.Production
{
    public class ProductionLog
    {
        public int Id { get; set; }

        public int LineId { get; set; }

        public int ProductId { get; set; }

        public int TargetParts { get; set; }

        public int ActualParts { get; set; }

        public int ScrapParts { get; set; }

        public int NrftParts { get; set; }

        [Required]
        [MaxLength(10)]
        public string HourInterval { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public int ShiftId { get; set; }

        public int? DeclaredDowntimeMinutes { get; set; }

        public int? DeclaredDowntimeReasonId { get; set; }

        public int? SystemStopMinutes { get; set; }

        public bool JustificationRequired { get; set; }

        public string? JustificationReason { get; set; }

        public int? ProductionWorkOrderId { get; set; }

        // Navigation properties
        public Line Line { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public Shift Shift { get; set; } = null!;
        public BreakdownReason? DeclaredDowntimeReason { get; set; }
        public ProductionWorkOrder? ProductionWorkOrder { get; set; }
        public ICollection<ProductionLogDefect> Defects { get; set; } = new List<ProductionLogDefect>();
    }
}
