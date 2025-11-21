using System.ComponentModel.DataAnnotations;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Planning
{
    public class ProductionWorkOrder
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string WorkOrderNumber { get; set; } = string.Empty;

        public int ProductId { get; set; }

        public int LineId { get; set; }

        public int PlannedQuantity { get; set; }

        public int QuantityPlanned { get; set; }

        public int ProducedQuantity { get; set; }

        public int QuantityProduced { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime? PlannedStartDate { get; set; }

        public DateTime? PlannedEndDate { get; set; }

        public DateTime? ActualStartDate { get; set; }

        public DateTime? ActualEndDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Product Product { get; set; } = null!;
        public Line Line { get; set; } = null!;
        public ICollection<Production.ProductionLog> ProductionLogs { get; set; } = new List<Production.ProductionLog>();
    }
}
