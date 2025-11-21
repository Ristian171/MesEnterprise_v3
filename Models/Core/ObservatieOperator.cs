using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Core
{
    public class ObservatieOperator
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        public DateTime DataOra { get; set; } = DateTime.UtcNow;

        public int LineId { get; set; }

        public int ProductId { get; set; }

        public int? UserId { get; set; }

        // Navigation properties
        public Line Line { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public User? User { get; set; }
    }
}
