using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace MesEnterprise.Models.Core
{
    public class Line
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public int? DepartmentId { get; set; }

        [MaxLength(100)]
        public string? ScanIdentifier { get; set; }

        public bool HasLiveScanning { get; set; }
        
        // New field for Live Scan feature - indicates if Live Scan is currently enabled for this line
        public bool LiveScanEnabled { get; set; }

        public decimal CostOperarePeOra { get; set; }

        [Required]
        [MaxLength(50)]
        public string DataAcquisitionMode { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        
        // New field for OEE Target feature
        public double? OeeTarget { get; set; }

        // Navigation properties
        public Department? Department { get; set; }
    }
}