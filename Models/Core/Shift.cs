using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Core
{
    public class Shift
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<ShiftBreak> Breaks { get; set; } = new List<ShiftBreak>();
    }
}
