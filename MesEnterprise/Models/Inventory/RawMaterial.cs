using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace MesEnterprise.Models.Inventory
{
    public class RawMaterial
    {
        [Key]
        public int Id { get; set; }

        public bool IsActive { get; set; }

        [MaxLength(100)]
        public string? MaterialCode { get; set; }

        public decimal MinimumStock { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        public decimal QuantityInStock { get; set; }

        [MaxLength(50)]
        public string? Unit { get; set; }

        public decimal UnitCost { get; set; }

    }
}