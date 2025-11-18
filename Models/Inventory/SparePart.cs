using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Inventory
{
    public class SparePart
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string PartNumber { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        public int QuantityInStock { get; set; }

        public int MinimumStock { get; set; }

        public bool IsActive { get; set; } = true;

        public decimal UnitCost { get; set; }
    }
}
