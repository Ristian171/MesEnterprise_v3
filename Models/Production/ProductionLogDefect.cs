using MesEnterprise.Models.Quality;

namespace MesEnterprise.Models.Production
{
    public class ProductionLogDefect
    {
        public int Id { get; set; }

        public int ProductionLogId { get; set; }
        public ProductionLog? ProductionLog { get; set; }

        public int DefectCodeId { get; set; }
        public DefectCode? DefectCode { get; set; }

        public int Quantity { get; set; }
    }
}
