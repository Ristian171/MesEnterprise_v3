using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Quality
{
    public class DefectCode
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public int DefectCategoryId { get; set; }
        public DefectCategory? Category { get; set; }
    }
}
