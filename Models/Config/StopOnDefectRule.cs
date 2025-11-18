using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Config
{
    public class StopOnDefectRule
    {
        public int Id { get; set; }
        public int LineId { get; set; }
        public int? ProductId { get; set; }
        public int MaxConsecutiveScrap { get; set; }
        public int MaxConsecutiveNRFT { get; set; }
        public int MaxNrftPerHour { get; set; }

        // Navigation properties
        public Line Line { get; set; } = null!;
        public Product? Product { get; set; }
    }
}
