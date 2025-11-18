using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Core;
using MesEnterprise.Models.Maintenance;

namespace MesEnterprise.Models.Maintenance
{
    public class InterventieTichet
    {
        [Key]
        public int Id { get; set; }

        public DateTime? CAPADueDate { get; set; }

        public string? CorrectiveAction { get; set; }

        public decimal? CostManopera { get; set; }

        public decimal? CostPiese { get; set; }

        public DateTime DataRaportareOperator { get; set; }

        public DateTime? DataStartInterventie { get; set; }

        public DateTime? DataStopInterventie { get; set; }

        public int? DefectiuneIdentificataId { get; set; }

        public string? DefectiuneTextLiber { get; set; }

        public int? DurataMinute { get; set; }

        public int EquipmentId { get; set; }

        public bool InfluenteazaProdusul { get; set; }

        public int LineId { get; set; }

        public string? OperatorNume { get; set; }

        public string? PreventiveAction { get; set; }

        public int? ProblemaRaportataId { get; set; }

        public int? ProductId { get; set; }

        public string? RootCause { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        public string? TehnicianNume { get; set; }

        public Guid UnicIdTicket { get; set; }

        // Navigation properties
        public Equipment? Equipment { get; set; }
        public Line? Line { get; set; }
        public Product? Product { get; set; }
        public ProblemaRaportata? ProblemaRaportata { get; set; }
        public User? TehnicianAsignat { get; set; }
    }
}