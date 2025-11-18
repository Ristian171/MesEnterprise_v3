using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Core
{
    public class Shift
    {
        [Key]
        public int Id { get; set; }

        public TimeSpan EndTime { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }

        public TimeSpan StartTime { get; set; }

        // Navigation properties
        public ICollection<ShiftBreak> Breaks { get; set; } = new List<ShiftBreak>();
    }
}