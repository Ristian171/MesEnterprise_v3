namespace MesEnterprise.Models.Core
{
    public class Shift
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        // Navigation properties
        public ICollection<ShiftBreak> Breaks { get; set; } = new List<ShiftBreak>();
    }
}
