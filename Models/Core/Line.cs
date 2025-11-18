namespace MesEnterprise.Models.Core
{
    public class Line
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public bool IsActive { get; set; }
        public string? ScanIdentifier { get; set; }
        public bool HasLiveScanning { get; set; }
        public string DataAcquisitionMode { get; set; } = string.Empty;
        public decimal CostOperarePeOra { get; set; }

        // Navigation properties
        public Department? Department { get; set; }
        public ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
    }
}
