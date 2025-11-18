using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Inventory
{
    public class RawMaterial
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string MaterialCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        public decimal QuantityInStock { get; set; }

        public decimal MinimumStock { get; set; }

        [Required, MaxLength(50)]
        public string UnitOfMeasure { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public decimal UnitCost { get; set; }
    }
}
