using System.ComponentModel.DataAnnotations;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Production
{
    public class ChangeoverLog
    {
        public int Id { get; set; }

        public int LineId { get; set; }
        public Line? Line { get; set; }

        public int? FromProductId { get; set; }
        public Product? FromProduct { get; set; }

        public int ToProductId { get; set; }
        public Product? ToProduct { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int? DurationMinutes { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
