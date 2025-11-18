using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Maintenance
{
    public class InterventieTichet
    {
        public int Id { get; set; }
        public Guid UnicIdTicket { get; set; }
        public int LineId { get; set; }
        public int EquipmentId { get; set; }
        public int? ProductId { get; set; }
        public DateTime DataRaportareOperator { get; set; }
        public string? OperatorNume { get; set; }
        public int? ProblemaRaportataId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? DataStartInterventie { get; set; }
        public DateTime? DataStopInterventie { get; set; }
        public int? DurataMinute { get; set; }
        public string? TehnicianNume { get; set; }
        public int? DefectiuneIdentificataId { get; set; }
        public string? DefectiuneTextLiber { get; set; }
        public bool InfluenteazaProdusul { get; set; }
        public string? RootCause { get; set; }
        public string? CorrectiveAction { get; set; }
        public string? PreventiveAction { get; set; }
        public DateTime? CAPADueDate { get; set; }
        public decimal? CostPiese { get; set; }
        public decimal? CostManopera { get; set; }

        // Navigation properties
        public Line Line { get; set; } = null!;
        public Equipment Equipment { get; set; } = null!;
        public Product? Product { get; set; }
        public ProblemaRaportata? ProblemaRaportata { get; set; }
        public DefectiuneIdentificata? DefectiuneIdentificata { get; set; }
    }
}
