using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Core
{
    public class Equipment
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? AssetNumber { get; set; }

        public int? LineId { get; set; }

        public decimal OperatingHours { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Line? Line { get; set; }
    }
}
