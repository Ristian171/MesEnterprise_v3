using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Quality
{
    public class DefectCode
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        public int DefectCategoryId { get; set; }
        public DefectCategory? Category { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(50)]
        public string? Severity { get; set; }
    }
}
