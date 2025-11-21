using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Quality
{
    public class DefectCategory
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Color { get; set; } = "#000000";

        // Navigation properties
        public ICollection<DefectCode> DefectCodes { get; set; } = new List<DefectCode>();
    }
}
