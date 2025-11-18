using System.ComponentModel.DataAnnotations;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Maintenance
{
    public class InterventieTichet
    {
        public int Id { get; set; }

        public Guid UnicIdTicket { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Open";

        public int LineId { get; set; }

        public int EquipmentId { get; set; }

        public int? ProductId { get; set; }

        public string? OperatorNume { get; set; }

        public DateTime DataRaportareOperator { get; set; } = DateTime.UtcNow;

        public string? TehnicianNume { get; set; }

        public DateTime? DataStartInterventie { get; set; }

        public DateTime? DataStopInterventie { get; set; }

        public int? ProblemaRaportataId { get; set; }

        public int? DefectiuneIdentificataId { get; set; }

        public string? DefectiuneTextLiber { get; set; }

        public bool InfluenteazaProdusul { get; set; }

        public string? RootCause { get; set; }

        public string? CorrectiveAction { get; set; }

        public string? PreventiveAction { get; set; }

        public DateTime? CapaDueDate { get; set; }

        public decimal? CostPiese { get; set; }

        public decimal? CostManopera { get; set; }

        public int? DurataMinute { get; set; }

        // Navigation properties
        public Line Line { get; set; } = null!;
        public Equipment Equipment { get; set; } = null!;
        public Product? Product { get; set; }
        public ProblemaRaportata? ProblemaRaportata { get; set; }
        public DefectiuneIdentificata? DefectiuneIdentificata { get; set; }
    }
}
