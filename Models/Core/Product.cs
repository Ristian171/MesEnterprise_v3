using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Core
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? PartNumber { get; set; }

        public decimal? StandardCycleTime { get; set; }

        public decimal? CycleTimeSeconds { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
