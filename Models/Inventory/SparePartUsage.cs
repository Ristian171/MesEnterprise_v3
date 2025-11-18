using System.ComponentModel.DataAnnotations;
using MesEnterprise.Models.Maintenance;

namespace MesEnterprise.Models.Inventory
{
    /// <summary>
    /// Înregistrare folosire piese de schimb în intervenții tehnice
    /// </summary>
    public class SparePartUsage
    {
        public int Id { get; set; }

        public int SparePartId { get; set; }
        public SparePart? SparePart { get; set; }

        public int InterventieTichetId { get; set; }
        public InterventieTichet? InterventieTichet { get; set; }

        public int QuantityUsed { get; set; }

        public DateTime UsedAt { get; set; } = DateTime.UtcNow;

        public int? UsedByUserId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
