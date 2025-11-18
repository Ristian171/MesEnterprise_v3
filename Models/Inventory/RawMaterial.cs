using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Inventory
{
    public class RawMaterial
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string MaterialCode { get; set; } = string.Empty;

        [Required, MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        public decimal ActualStock { get; set; }

        public decimal ReorderStock { get; set; }

        [Required, MaxLength(50)]
        public string UnitOfMeasure { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public decimal? UnitCost { get; set; }
    }
}
