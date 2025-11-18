using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using MesEnterprise.Models.Core;

namespace MesEnterprise.Models.Core
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string? FullName { get; set; }

        public bool IsActive { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public string? PasswordHash { get; set; }

        public int? RoleId { get; set; }

        [MaxLength(100)]
        public string? Username { get; set; }

        // Navigation properties
        public Role? Role { get; set; }
    }
}