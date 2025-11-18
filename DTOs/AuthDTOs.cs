using System.ComponentModel.DataAnnotations;

namespace MesEnterprise.DTOs
{
    public class LoginRequest
    {
        [Required]
        public required string Username { get; set; }
        
        [Required]
        public required string Password { get; set; }
    }

    public class RegisterRequest
    {
        [Required]
        public required string Username { get; set; }
        
        [Required]
        public required string Password { get; set; }
        
        public string? Role { get; set; }
        public int? RoleId { get; set; }
    }

    public class LoginResponse
    {
        public required string Token { get; set; }
        public required string Username { get; set; }
        public string? Role { get; set; }
    }
}
