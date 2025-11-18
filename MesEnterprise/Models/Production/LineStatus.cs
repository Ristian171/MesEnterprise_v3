using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Production
{
    public class LineStatus
    {
        [Key]
        public int Id { get; set; }

        public int? CurrentShiftId { get; set; }

        public DateTime LastStatusChange { get; set; }

        public int LineId { get; set; }

        public int? ProductId { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        // Navigation properties
        public Line? Line { get; set; }
        public Product? Product { get; set; }
        public Shift? CurrentShift { get; set; }
    }
}