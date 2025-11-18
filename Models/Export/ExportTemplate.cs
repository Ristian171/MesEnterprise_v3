namespace MesEnterprise.Models.Export
{
    public class ExportTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TemplateType { get; set; } = string.Empty;
        public string? Configuration { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
