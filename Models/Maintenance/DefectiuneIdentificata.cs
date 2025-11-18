namespace MesEnterprise.Models.Maintenance
{
    public class DefectiuneIdentificata
    {
        public int Id { get; set; }
        public string Nume { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<ProblemaDefectiuneCorelatie> Corelatii { get; set; } = new List<ProblemaDefectiuneCorelatie>();
    }
}
