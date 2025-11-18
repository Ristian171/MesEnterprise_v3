using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Production;
using MesEnterprise.Models.Quality;

namespace MesEnterprise.Models.Quality
{
    public class ProductionLogQualityCheck
    {
        [Key]
        public int Id { get; set; }

        public string? Notes { get; set; }

        public int ProductionLogId { get; set; }

        public int QualityTestId { get; set; }

        [MaxLength(50)]
        public string? Result { get; set; }

        public DateTime TestedAt { get; set; }

        // Navigation properties
        public ProductionLog? ProductionLog { get; set; }
        public QualityTest? QualityTest { get; set; }
    }
}