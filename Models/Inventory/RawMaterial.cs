using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Inventory
{
    public class RawMaterial
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? MaterialCode { get; set; }

        public decimal QuantityOnHand { get; set; }

        public decimal QuantityInStock { get; set; }

        public decimal MinimumQuantity { get; set; }

        public decimal MinimumStock { get; set; }

        [MaxLength(50)]
        public string? Unit { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        public decimal? UnitCost { get; set; }

        [MaxLength(100)]
        public string? Supplier { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
