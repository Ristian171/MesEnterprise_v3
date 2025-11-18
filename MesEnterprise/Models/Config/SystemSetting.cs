using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace MesEnterprise.Models.Config
{
    public class SystemSetting
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Key { get; set; }

        public DateTime UpdatedAt { get; set; }

        [MaxLength(1000)]
        public string? Value { get; set; }

    }
}