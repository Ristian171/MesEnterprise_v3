using MesEnterprise.Models.Production;

namespace MesEnterprise.Models.Quality
{
    public class ProductionLogQualityCheck
    {
        public int Id { get; set; }
        public int ProductionLogId { get; set; }
        public int QualityTestId { get; set; }
        public string Result { get; set; } = string.Empty;
        public DateTime TestedAt { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public ProductionLog ProductionLog { get; set; } = null!;
        public QualityTest QualityTest { get; set; } = null!;
    }
}
