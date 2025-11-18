using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Quality
{
    public class DefectCategory
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<DefectCode> DefectCodes { get; set; } = new List<DefectCode>();
    }
}
