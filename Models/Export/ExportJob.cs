using System.ComponentModel.DataAnnotations;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Export
{
    public class ExportJob
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string JobName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ExportType { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public string? Parameters { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public int? CreatedByUserId { get; set; }

        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        // Navigation properties
        public User? CreatedByUser { get; set; }
    }
}
