using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Production;
using MesEnterprise.Models.Quality;

namespace MesEnterprise.Models.Production
{
    public class ProductionLogDefect
    {
        [Key]
        public int Id { get; set; }

        public int DefectCodeId { get; set; }

        public int ProductionLogId { get; set; }

        public int Quantity { get; set; }

        // Navigation properties
        public ProductionLog? ProductionLog { get; set; }
        public DefectCode? DefectCode { get; set; }
    }
}