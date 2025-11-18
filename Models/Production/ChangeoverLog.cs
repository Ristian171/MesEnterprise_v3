using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Production
{
    public class ChangeoverLog
    {
        public int Id { get; set; }
        public int LineId { get; set; }
        public int ProductFromId { get; set; }
        public int ProductToId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        // Navigation properties
        public Line Line { get; set; } = null!;
        public Product ProductFrom { get; set; } = null!;
        public Product ProductTo { get; set; } = null!;
    }
}
