namespace MesEnterprise.Models.Maintenance
{
    public class ProblemaDefectiuneCorelatie
    {
        public int ProblemaRaportataId { get; set; }
        public int DefectiuneIdentificataId { get; set; }

        // Navigation properties
        public ProblemaRaportata ProblemaRaportata { get; set; } = null!;
        public DefectiuneIdentificata DefectiuneIdentificata { get; set; } = null!;
    }
}
