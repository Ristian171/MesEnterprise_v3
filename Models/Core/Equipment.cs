using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Core
{
    public class Equipment
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Code { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int LineId { get; set; }
        public Line? Line { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastMaintenanceDate { get; set; }

        public decimal OreFunctionare { get; set; }
    }
}
