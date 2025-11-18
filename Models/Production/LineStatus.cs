using System.ComponentModel.DataAnnotations;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Production
{
    public class LineStatus
    {
        public int Id { get; set; }

        public int LineId { get; set; }
        public Line? Line { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Idle";

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public int? CurrentShiftId { get; set; }
        public Shift? CurrentShift { get; set; }

        public DateTime LastStatusChange { get; set; } = DateTime.UtcNow;
    }
}
