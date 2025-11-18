using System.ComponentModel.DataAnnotations;
using MesEnterprise.Models.Production;

namespace MesEnterprise.Models.Quality
{
    public class ProductionLogQualityCheck
    {
        public int Id { get; set; }

        public int ProductionLogId { get; set; }
        public ProductionLog? ProductionLog { get; set; }

        public int QualityTestId { get; set; }
        public QualityTest? QualityTest { get; set; }

        public decimal? MeasuredValue { get; set; }

        [MaxLength(50)]
        public string? Result { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
