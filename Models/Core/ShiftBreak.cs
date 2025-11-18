using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Core
{
    public class ShiftBreak
    {
        public int Id { get; set; }

        public int ShiftId { get; set; }
        public Shift? Shift { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public TimeSpan BreakTime { get; set; }

        public int DurationMinutes { get; set; }
    }
}
