using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Quality;

namespace MesEnterprise.Models.Quality
{
    public class DefectCode
    {
        [Key]
        public int Id { get; set; }

        public int DefectCategoryId { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        // Navigation properties
        public DefectCategory? Category { get; set; }
    }
}