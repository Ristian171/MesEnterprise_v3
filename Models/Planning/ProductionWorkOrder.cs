using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Planning
{
    public class ProductionWorkOrder
    {
        public int Id { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public int LineId { get; set; }
        public int ProductId { get; set; }
        public int PlannedQuantity { get; set; }
        public int ProducedQuantity { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Line Line { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
