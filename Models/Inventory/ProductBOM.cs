using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Inventory
{
    public class ProductBOM
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int RawMaterialId { get; set; }
        public RawMaterial? RawMaterial { get; set; }

        public decimal QuantityPerUnit { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
