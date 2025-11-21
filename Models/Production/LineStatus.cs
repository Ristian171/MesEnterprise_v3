using System.ComponentModel.DataAnnotations;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Production
{
    public class LineStatus
    {
        public int Id { get; set; }

        public int LineId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        public int? ProductId { get; set; }

        [MaxLength(100)]
        public string? CurrentOrder { get; set; }

        public int? CurrentShiftId { get; set; }

        public DateTime? LastStatusChange { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Line Line { get; set; } = null!;
        public Product? Product { get; set; }
        public Shift? CurrentShift { get; set; }
    }
}
