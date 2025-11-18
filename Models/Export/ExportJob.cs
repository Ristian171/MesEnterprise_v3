using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Export
{
    public class ExportJob
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? QueryConfiguration { get; set; }
        public string? FilePath { get; set; }
        public string? ErrorMessage { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        public User? CreatedByUser { get; set; }
    }
}
