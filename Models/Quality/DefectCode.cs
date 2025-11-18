namespace MesEnterprise.Models.Quality
{
    public class DefectCode
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DefectCategoryId { get; set; }

        // Navigation properties
        public DefectCategory Category { get; set; } = null!;
    }
}
