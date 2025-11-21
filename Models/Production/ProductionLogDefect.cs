using MesEnterprise.Models.Quality;

namespace MesEnterprise.Models.Production
{
    public class ProductionLogDefect
    {
        public int Id { get; set; }

        public int ProductionLogId { get; set; }

        public int DefectCodeId { get; set; }

        public int Quantity { get; set; }

        // Navigation properties
        public ProductionLog ProductionLog { get; set; } = null!;
        public DefectCode DefectCode { get; set; } = null!;
    }
}
