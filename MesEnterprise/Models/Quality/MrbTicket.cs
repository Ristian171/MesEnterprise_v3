using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Production;

namespace MesEnterprise.Models.Quality
{
    public class MrbTicket
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? DefectDescription { get; set; }

        public int DefectiveQuantity { get; set; }

        public string? Disposition { get; set; }

        public int ProductId { get; set; }

        public int? ProductionLogId { get; set; }

        public DateTime? ResolvedAt { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(50)]
        public string? TicketNumber { get; set; }

        // Navigation properties
        public ProductionLog? ProductionLog { get; set; }
    }
}