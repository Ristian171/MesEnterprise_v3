using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Core
{
    public class ObservatieOperator
    {
        [Key]
        public int Id { get; set; }

        public DateTime DataOra { get; set; }

        public int LineId { get; set; }

        public int ProductId { get; set; }

        public string? Text { get; set; }

        public int? UserId { get; set; }

        // Navigation properties
        public Line? Line { get; set; }
        public Product? Product { get; set; }
        public User? User { get; set; }
    }
}