using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace MesEnterprise.Models.Export
{
    public class ExportTemplate
    {
        [Key]
        public int Id { get; set; }

        public string? Configuration { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? TemplateType { get; set; }

    }
}