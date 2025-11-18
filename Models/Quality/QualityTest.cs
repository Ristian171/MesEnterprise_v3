using System.ComponentModel.DataAnnotations;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Quality
{
    public class QualityTest
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string TestName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int? ProductId { get; set; }

        [MaxLength(50)]
        public string? TestType { get; set; }

        public int FrequencyMinutes { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Product? Product { get; set; }
    }
}
