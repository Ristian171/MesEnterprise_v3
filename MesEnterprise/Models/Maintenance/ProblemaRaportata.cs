using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Maintenance;

namespace MesEnterprise.Models.Maintenance
{
    public class ProblemaRaportata
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(500)]
        public string? Nume { get; set; }

        // Navigation properties
        public ICollection<ProblemaDefectiuneCorelatie> Corelatii { get; set; } = new List<ProblemaDefectiuneCorelatie>();
    }
}