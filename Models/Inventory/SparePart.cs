using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Inventory
{
    public class SparePart
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? PartNumber { get; set; }

        public int QuantityOnHand { get; set; }

        public int QuantityInStock { get; set; }

        public int MinimumQuantity { get; set; }

        public int MinimumStock { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        public decimal? UnitCost { get; set; }

        [MaxLength(100)]
        public string? Supplier { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
