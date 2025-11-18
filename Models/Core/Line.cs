using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Core
{
    public class Line
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ScanIdentifier { get; set; }

        public int? DepartmentId { get; set; }

        public decimal? HourlyCost { get; set; }

        [MaxLength(50)]
        public string? DataAcquisitionMode { get; set; }

        public bool HasLiveScanning { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Department? Department { get; set; }
    }
}
