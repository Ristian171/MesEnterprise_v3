using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Core
{
    public class ShiftBreak
    {
        [Key]
        public int Id { get; set; }

        public TimeSpan BreakTime { get; set; }

        public int DurationMinutes { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }

        public int ShiftId { get; set; }

        // Navigation properties
        public Shift? Shift { get; set; }
    }
}