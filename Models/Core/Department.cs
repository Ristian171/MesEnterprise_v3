namespace MesEnterprise.Models.Core
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public ICollection<Line> Lines { get; set; } = new List<Line>();
    }
}
