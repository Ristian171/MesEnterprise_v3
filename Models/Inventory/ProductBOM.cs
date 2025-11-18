using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Inventory
{
    public class ProductBOM
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int RawMaterialId { get; set; }
        public decimal QuantityPerUnit { get; set; }

        // Navigation properties
        public Product Product { get; set; } = null!;
        public RawMaterial RawMaterial { get; set; } = null!;
    }
}
