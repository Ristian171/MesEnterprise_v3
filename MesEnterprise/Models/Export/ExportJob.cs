using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Export
{
    public class ExportJob
    {
        [Key]
        public int Id { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? CreatedByUserId { get; set; }

        public string? ErrorMessage { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(50)]
        public string? Format { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        public string? QueryConfiguration { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        // Navigation properties
        public User? CreatedByUser { get; set; }
    }
}