using System.ComponentModel.DataAnnotations;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Quality
{
    public class QualityTest
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        [MaxLength(50)]
        public string? TestType { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(50)]
        public string? UnitOfMeasure { get; set; }

        public decimal? MinValue { get; set; }

        public decimal? MaxValue { get; set; }
    }
}
