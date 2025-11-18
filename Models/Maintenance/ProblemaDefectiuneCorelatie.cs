namespace MesEnterprise.Models.Maintenance
{
    public class ProblemaDefectiuneCorelatie
    {
        public int ProblemaRaportataId { get; set; }
        public ProblemaRaportata ProblemaRaportata { get; set; } = null!;

        public int DefectiuneIdentificataId { get; set; }
        public DefectiuneIdentificata DefectiuneIdentificata { get; set; } = null!;

        public int Frecventa { get; set; }
    }
}
