using System.ComponentModel.DataAnnotations;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Inventory
{
    public class SparePart
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string PartNumber { get; set; } = string.Empty;

        [Required, MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        public int ActualStock { get; set; }

        public int ReorderStock { get; set; }

        public int? EquipmentId { get; set; }
        public Equipment? Equipment { get; set; }

        public int? LineId { get; set; }
        public Line? Line { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastUsedDate { get; set; }

        [MaxLength(50)]
        public string? UnitOfMeasure { get; set; }

        public decimal? UnitCost { get; set; }
    }
}
