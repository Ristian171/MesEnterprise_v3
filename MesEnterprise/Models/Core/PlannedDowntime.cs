using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Core
{
    public class PlannedDowntime
    {
        [Key]
        public int Id { get; set; }

        public int LineId { get; set; }

        public double MinutesPerHour { get; set; }

        public int ProductId { get; set; }

        // Navigation properties
        public Line? Line { get; set; }
        public Product? Product { get; set; }
    }
}