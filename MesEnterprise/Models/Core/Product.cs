using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Core;
using MesEnterprise.Models.Inventory;

namespace MesEnterprise.Models.Core
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string? Code { get; set; }

        public DateTime CreatedAt { get; set; }

        public double CycleTimeSeconds { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        // Navigation properties
        public ICollection<ProductBOM> BOMs { get; set; } = new List<ProductBOM>();
    }
}