using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Quality
{
    public class DefectCode
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public int DefectCategoryId { get; set; }

        [MaxLength(50)]
        public string? Severity { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public DefectCategory DefectCategory { get; set; } = null!;
        public DefectCategory Category { get; set; } = null!;
    }
}
