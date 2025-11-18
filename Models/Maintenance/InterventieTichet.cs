using System.ComponentModel.DataAnnotations;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Maintenance
{
    public class InterventieTichet
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string UnicIdTicket { get; set; } = string.Empty;

        public DateTime DataRaportareOperator { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(200)]
        public string OperatorNume { get; set; } = string.Empty;

        public int LineId { get; set; }
        public Line? Line { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public int? EquipmentId { get; set; }
        public Equipment? Equipment { get; set; }

        public int? ProblemaRaportataId { get; set; }
        public ProblemaRaportata? ProblemaRaportata { get; set; }

        public string? DescriereOperator { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Deschis";

        public DateTime? DataPreluareTehnic { get; set; }

        [MaxLength(200)]
        public string? TehnicNume { get; set; }

        public int? DefectiuneIdentificataId { get; set; }
        public DefectiuneIdentificata? DefectiuneIdentificata { get; set; }

        public string? CauzaProbabila { get; set; }

        public string? ActiuniIntreprinse { get; set; }

        public DateTime? DataInchidere { get; set; }

        public int? DurataInterventie { get; set; }
    }
}
