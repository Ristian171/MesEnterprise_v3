using System.ComponentModel.DataAnnotations;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Quality
{
    public class MrbTicket
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string TicketNumber { get; set; } = string.Empty;

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public string? Description { get; set; }

        public string? RootCause { get; set; }

        public string? DispositionAction { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public int? ResolvedByUserId { get; set; }
    }
}
