using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Planning
{
    public class ProductionWorkOrder
    {
        [Key]
        public int Id { get; set; }

        public DateTime? ActualEndDate { get; set; }

        public DateTime? ActualStartDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public int LineId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime? PlannedEndDate { get; set; }

        public int PlannedQuantity { get; set; }

        public DateTime PlannedStartDate { get; set; }

        public int ProducedQuantity { get; set; }

        public int ProductId { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(100)]
        public string? WorkOrderNumber { get; set; }

        // Navigation properties
        public Line? Line { get; set; }
        public Product? Product { get; set; }
    }
}