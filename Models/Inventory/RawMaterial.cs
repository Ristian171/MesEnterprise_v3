namespace MesEnterprise.Models.Inventory
{
    public class RawMaterial
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MaterialCode { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal QuantityInStock { get; set; }
        public decimal MinimumStock { get; set; }
        public decimal UnitCost { get; set; }
        public bool IsActive { get; set; }
    }
}
