using System.ComponentModel.DataAnnotations;
using MesEnterprise.Models.Core;
using MesEnterprise.Models.Production;

namespace MesEnterprise.Models.Quality
{
    public class MrbTicket
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string TicketNumber { get; set; } = string.Empty;

        public int? ProductionLogId { get; set; }

        public int ProductId { get; set; }

        public int DefectiveQuantity { get; set; }

        public string? DefectDescription { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Open";

        public string? Disposition { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }

        // Navigation properties
        public ProductionLog? ProductionLog { get; set; }
        public Product Product { get; set; } = null!;
    }
}
