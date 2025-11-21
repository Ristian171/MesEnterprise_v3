using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Core
{
    public class Department
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Code { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<Line> Lines { get; set; } = new List<Line>();
    }
}
