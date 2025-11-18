using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Core
{
    public class Permission
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Module { get; set; }

        // Navigation properties
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
