namespace MesEnterprise.Models.Inventory
{
    public class SparePart
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public int QuantityInStock { get; set; }
        public int MinimumStock { get; set; }
        public decimal UnitCost { get; set; }
        public bool IsActive { get; set; }
    }
}
