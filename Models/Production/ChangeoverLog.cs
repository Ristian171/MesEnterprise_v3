using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Production
{
    public class ChangeoverLog
    {
        public int Id { get; set; }

        public int LineId { get; set; }
        public Line? Line { get; set; }

        public int ProductFromId { get; set; }
        public Product? ProductFrom { get; set; }

        public int ProductToId { get; set; }
        public Product? ProductTo { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }
}
