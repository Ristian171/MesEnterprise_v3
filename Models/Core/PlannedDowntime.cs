namespace MesEnterprise.Models.Core
{
    public class PlannedDowntime
    {
        public int Id { get; set; }
        public int LineId { get; set; }
        public int ProductId { get; set; }
        public double MinutesPerHour { get; set; }

        // Navigation properties
        public Line Line { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
