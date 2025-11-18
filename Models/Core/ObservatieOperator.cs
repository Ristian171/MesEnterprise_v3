namespace MesEnterprise.Models.Core
{
    public class ObservatieOperator
    {
        public int Id { get; set; }
        public int LineId { get; set; }
        public int ProductId { get; set; }
        public int? UserId { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime DataOra { get; set; }

        // Navigation properties
        public Line Line { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public User? User { get; set; }
    }
}
