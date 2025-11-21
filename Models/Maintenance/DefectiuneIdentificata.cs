using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Maintenance
{
    public class DefectiuneIdentificata
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Nume { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<ProblemaDefectiuneCorelatie> Corelatii { get; set; } = new List<ProblemaDefectiuneCorelatie>();
    }
}
