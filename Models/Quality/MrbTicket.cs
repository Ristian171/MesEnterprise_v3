using MesEnterprise.Models.Core;
using MesEnterprise.Models.Production;

namespace MesEnterprise.Models.Quality
{
    public class MrbTicket
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public int? ProductionLogId { get; set; }
        public int DefectiveQuantity { get; set; }
        public string? DefectDescription { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Disposition { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        // Navigation properties
        public Product Product { get; set; } = null!;
        public ProductionLog? ProductionLog { get; set; }
    }
}
