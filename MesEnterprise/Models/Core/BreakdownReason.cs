using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace MesEnterprise.Models.Core
{
    public class BreakdownReason
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

    }
}