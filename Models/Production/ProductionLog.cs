using System.ComponentModel.DataAnnotations;
using MesEnterprise.Models.Core;
using MesEnterprise.Models.Planning;

namespace MesEnterprise.Models.Production
{
    public class ProductionLog
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(10)]
        public string HourInterval { get; set; } = string.Empty;

        public int LineId { get; set; }
        public Line? Line { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int ShiftId { get; set; }
        public Shift? Shift { get; set; }

        public int ActualParts { get; set; }

        public int ScrapParts { get; set; }

        public int NrftParts { get; set; }

        public int TargetParts { get; set; }

        public int? SystemStopMinutes { get; set; }

        public int? DeclaredDowntimeMinutes { get; set; }

        public int? DeclaredDowntimeReasonId { get; set; }
        public BreakdownReason? DeclaredDowntimeReason { get; set; }

        public bool JustificationRequired { get; set; }

        public string? JustificationReason { get; set; }

        public int? ProductionWorkOrderId { get; set; }
        public ProductionWorkOrder? ProductionWorkOrder { get; set; }

        public ICollection<ProductionLogDefect> DefectAllocations { get; set; } = new List<ProductionLogDefect>();
    }
}
