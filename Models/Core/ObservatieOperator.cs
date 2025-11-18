using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Core
{
    public class ObservatieOperator
    {
        public int Id { get; set; }

        public DateTime DataOra { get; set; } = DateTime.UtcNow;

        public int LineId { get; set; }
        public Line? Line { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Categorie { get; set; }
    }
}
