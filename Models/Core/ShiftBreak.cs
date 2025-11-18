namespace MesEnterprise.Models.Core
{
    public class ShiftBreak
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ShiftId { get; set; }
        public TimeSpan BreakTime { get; set; }
        public int DurationMinutes { get; set; }

        // Navigation properties
        public Shift Shift { get; set; } = null!;
    }
}
