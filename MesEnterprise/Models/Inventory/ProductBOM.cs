using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Core;
using MesEnterprise.Models.Inventory;

namespace MesEnterprise.Models.Inventory
{
    public class ProductBOM
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        public decimal QuantityPerUnit { get; set; }

        public int RawMaterialId { get; set; }

        // Navigation properties
        public Product? Product { get; set; }
        public RawMaterial? RawMaterial { get; set; }
    }
}