using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Quality
{
    public class QualityTest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public string? AcceptanceCriteria { get; set; }
        public int? ProductId { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public Product? Product { get; set; }
    }
}
