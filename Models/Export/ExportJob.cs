using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Export
{
    public class ExportJob
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string JobName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string ExportType { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        public string? ErrorMessage { get; set; }

        public int? RequestedByUserId { get; set; }

        public string? Parameters { get; set; }
    }
}
