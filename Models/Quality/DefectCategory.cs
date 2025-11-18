namespace MesEnterprise.Models.Quality
{
    public class DefectCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<DefectCode> DefectCodes { get; set; } = new List<DefectCode>();
    }
}
