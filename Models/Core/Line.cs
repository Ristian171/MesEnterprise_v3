using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Core
{
    public class Line
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        [MaxLength(100)]
        public string? ScanIdentifier { get; set; }

        [Required, MaxLength(50)]
        public string DataAcquisitionMode { get; set; } = "Manual";

        public bool IsActive { get; set; } = true;

        public bool HasLiveScanning { get; set; }

        public decimal CostOperarePeOra { get; set; }

        /// <summary>
        /// Target timp changeover în minute pentru această linie
        /// </summary>
        public int? ChangeoverTargetMinutes { get; set; }

        /// <summary>
        /// Target OEE (%) pentru această linie
        /// </summary>
        public decimal? TargetOEEPercent { get; set; }
    }
}
