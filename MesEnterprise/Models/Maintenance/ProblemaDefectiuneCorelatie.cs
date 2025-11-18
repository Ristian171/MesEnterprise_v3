using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Maintenance;

namespace MesEnterprise.Models.Maintenance
{
    public class ProblemaDefectiuneCorelatie
    {
        public int ProblemaRaportataId { get; set; }

        public int DefectiuneIdentificataId { get; set; }

        // Navigation properties
        public ProblemaRaportata? ProblemaRaportata { get; set; }
        public DefectiuneIdentificata? DefectiuneIdentificata { get; set; }
    }
}