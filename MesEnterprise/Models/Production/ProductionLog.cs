using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Core;
using MesEnterprise.Models.Quality;

namespace MesEnterprise.Models.Production
{
    public class ProductionLog
    {
        [Key]
        public int Id { get; set; }

        public int ActualParts { get; set; }

        public int? DeclaredDowntimeMinutes { get; set; }

        public int? DeclaredDowntimeReasonId { get; set; }

        [MaxLength(10)]
        public string? HourInterval { get; set; }

        public string? JustificationReason { get; set; }

        public bool JustificationRequired { get; set; }

        public int LineId { get; set; }

        public int NrftParts { get; set; }

        public int ProductId { get; set; }

        public int? ProductionWorkOrderId { get; set; }

        public int ScrapParts { get; set; }

        public int ShiftId { get; set; }

        public int? SystemStopMinutes { get; set; }

        public int TargetParts { get; set; }

        public DateTime Timestamp { get; set; }

        // Navigation properties
        public Line? Line { get; set; }
        public Product? Product { get; set; }
        public Shift? Shift { get; set; }
        public BreakdownReason? DeclaredDowntimeReason { get; set; }
        public ICollection<ProductionLogDefect> DefectAllocations { get; set; } = new List<ProductionLogDefect>();
        public ICollection<ProductionLogQualityCheck> QualityChecks { get; set; } = new List<ProductionLogQualityCheck>();
    }
}