namespace MesEnterprise.Models.Core
{
    public class Equipment
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int LineId { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public decimal OreFunctionare { get; set; }

        // Navigation properties
        public Line Line { get; set; } = null!;
    }
}
