using System.ComponentModel.DataAnnotations;
using MesEnterprise.Models.Core;
using MesEnterprise.Models.Production;

namespace MesEnterprise.Models.Quality
{
    public class ProductionLogQualityCheck
    {
        public int Id { get; set; }

        public int ProductionLogId { get; set; }

        public int QualityTestId { get; set; }

        public DateTime TestedAt { get; set; } = DateTime.UtcNow;

        public int? TestedByUserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Result { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Navigation properties
        public ProductionLog ProductionLog { get; set; } = null!;
        public QualityTest QualityTest { get; set; } = null!;
        public User? TestedByUser { get; set; }
    }
}
