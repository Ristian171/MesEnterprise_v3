using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Export
{
    public class ExportTemplate
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string TemplateName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string ExportType { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public string? TemplateConfiguration { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedAt { get; set; }
    }
}
