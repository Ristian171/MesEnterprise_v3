using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Quality;

namespace MesEnterprise.Models.Quality
{
    public class DefectCategory
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(20)]
        public string? Color { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        // Navigation properties
        public ICollection<DefectCode> DefectCodes { get; set; } = new List<DefectCode>();
    }
}