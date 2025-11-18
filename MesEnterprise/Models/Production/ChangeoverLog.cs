using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Production
{
    public class ChangeoverLog
    {
        [Key]
        public int Id { get; set; }

        public DateTime? EndTime { get; set; }

        public int LineId { get; set; }

        public int ProductFromId { get; set; }

        public int ProductToId { get; set; }

        public DateTime StartTime { get; set; }

        // Navigation properties
        public Line? Line { get; set; }
        public Product? ProductFrom { get; set; }
        public Product? ProductTo { get; set; }
    }
}