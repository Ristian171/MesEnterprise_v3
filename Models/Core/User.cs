using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.Models.Core
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? FullName { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        public int? RoleId { get; set; }
        public Role? Role { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }
    }
}
