using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Quality
{
    public class QualityTest
    {
        [Key]
        public int Id { get; set; }

        public string? AcceptanceCriteria { get; set; }

        public bool IsActive { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        public int? ProductId { get; set; }

        [MaxLength(50)]
        public string? TestType { get; set; }

        // Navigation properties
        public Product? Product { get; set; }
    }
}