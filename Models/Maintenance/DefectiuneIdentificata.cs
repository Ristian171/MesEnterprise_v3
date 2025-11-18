using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Maintenance
{
    public class DefectiuneIdentificata
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Nume { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<ProblemaDefectiuneCorelatie> Corelatii { get; set; } = new List<ProblemaDefectiuneCorelatie>();
    }
}
