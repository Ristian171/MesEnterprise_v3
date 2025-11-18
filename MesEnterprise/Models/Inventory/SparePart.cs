using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace MesEnterprise.Models.Inventory
{
    public class SparePart
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        public int MinimumStock { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? PartNumber { get; set; }

        public int QuantityInStock { get; set; }

        public decimal UnitCost { get; set; }

    }
}