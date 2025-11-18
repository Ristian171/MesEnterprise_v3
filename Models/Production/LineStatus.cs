using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Production
{
    public class LineStatus
    {
        public int Id { get; set; }
        public int LineId { get; set; }
        public int? ProductId { get; set; }
        public int? CurrentShiftId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime LastStatusChange { get; set; }

        // Navigation properties
        public Line Line { get; set; } = null!;
        public Product? Product { get; set; }
        public Shift? CurrentShift { get; set; }
    }
}
