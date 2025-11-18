using MesEnterprise.Data;
using MesEnterprise.Models.Core;
using MesEnterprise.Services;
using MesEnterprise.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MesEnterprise.Endpoints
{
    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder MapAuthApi(this IEndpointRouteBuilder app)
        {
            var authApi = app.MapGroup("/api/auth");
            var adminApi = app.MapGroup("/api/admin").RequireAuthorization("AdminOnly");

            // POST /api/auth/login
            authApi.MapPost("/login", [AllowAnonymous] async (
                [FromBody] LoginRequest req,
                MesDbContext db,
                PasswordService passwordService,
                TokenService tokenService) =>
            {
                var user = await db.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == req.Username.ToLower());

                if (user == null || user.PasswordHash == null || !passwordService.VerifyPassword(req.Password, user.PasswordHash))
                {
                    return Results.Json(new { Message = "Nume de utilizator sau parolă invalidă." }, statusCode: 401);
                }

                // Update last login time
                user.LastLoginAt = DateTime.UtcNow;
                await db.SaveChangesAsync();

                var roleName = user.Role?.Name ?? "Operator";
                var token = tokenService.GenerateToken(user, roleName);

                return Results.Ok(new
                {
                    Token = token,
                    Username = user.Username,
                    Role = roleName
                });
            });

            // GET /api/admin/users - List all users
            adminApi.MapGet("/users", async (MesDbContext db) =>
            {
                var users = await db.Users
                    .Include(u => u.Role)
                    .Select(u => new { 
                        u.Id, 
                        u.Username, 
                        Role = u.Role != null ? u.Role.Name : "No Role",
                        u.Email,
                        u.FullName,
                        u.IsActive,
                        u.CreatedAt,
                        u.LastLoginAt
                    })
                    .ToListAsync();
                return Results.Ok(users);
            });

            // POST /api/admin/users - Create user
            adminApi.MapPost("/users", async (
                [FromBody] RegisterRequest req,
                MesDbContext db,
                PasswordService passwordService) =>
            {
                if (string.IsNullOrEmpty(req.Password) || req.Password.Length < 6)
                {
                    return Results.BadRequest(new { Message = "Parola este obligatorie și trebuie să aibă minim 6 caractere." });
                }
                if (string.IsNullOrEmpty(req.Username) || req.Username.Contains(" "))
                {
                    return Results.BadRequest(new { Message = "Numele de utilizator este obligatoriu și nu poate conține spații." });
                }

                var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == req.Username.ToLower());
                if (existingUser != null)
                {
                    return Results.BadRequest(new { Message = "Numele de utilizator există deja." });
                }

                // Find role by name or ID
                Role? role = null;
                if (req.RoleId.HasValue)
                {
                    role = await db.Roles.FindAsync(req.RoleId.Value);
                }
                else if (!string.IsNullOrEmpty(req.Role))
                {
                    role = await db.Roles.FirstOrDefaultAsync(r => r.Name == req.Role);
                }

                var user = new User
                {
                    Username = req.Username,
                    PasswordHash = passwordService.HashPassword(req.Password),
                    RoleId = role?.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                db.Users.Add(user);
                await db.SaveChangesAsync();

                return Results.Created($"/api/admin/users/{user.Id}", new { 
                    user.Id, 
                    user.Username, 
                    Role = role?.Name 
                });
            });

            // PUT /api/admin/users/{id} - Update user
            adminApi.MapPut("/users/{id}", async (
                int id,
                [FromBody] RegisterRequest req,
                MesDbContext db,
                PasswordService passwordService) =>
            {
                var user = await db.Users.FindAsync(id);
                if (user == null)
                {
                    return Results.NotFound();
                }

                if (string.IsNullOrEmpty(req.Username) || req.Username.Contains(" "))
                {
                    return Results.BadRequest(new { Message = "Numele de utilizator este obligatoriu și nu poate conține spații." });
                }

                var existingUser = await db.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == req.Username.ToLower() && u.Id != id);
                if (existingUser != null)
                {
                    return Results.BadRequest(new { Message = "Numele de utilizator există deja." });
                }

                user.Username = req.Username;

                // Update role
                if (req.RoleId.HasValue)
                {
                    user.RoleId = req.RoleId.Value;
                }
                else if (!string.IsNullOrEmpty(req.Role))
                {
                    var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == req.Role);
                    user.RoleId = role?.Id;
                }

                // Update password only if provided
                if (!string.IsNullOrEmpty(req.Password))
                {
                    if (req.Password.Length < 6)
                    {
                        return Results.BadRequest(new { Message = "Parola trebuie să aibă minim 6 caractere." });
                    }
                    user.PasswordHash = passwordService.HashPassword(req.Password);
                }

                await db.SaveChangesAsync();
                
                var updatedUser = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
                return Results.Ok(new { 
                    updatedUser.Id, 
                    updatedUser.Username, 
                    Role = updatedUser.Role?.Name 
                });
            });

            // DELETE /api/admin/users/{id} - Delete user
            adminApi.MapDelete("/users/{id}", [Authorize("AdminOnly")] async (
                int id, 
                MesDbContext db, 
                ClaimsPrincipal principal) =>
            {
                var user = await db.Users.FindAsync(id);
                if (user == null) return Results.NotFound();

                // Protection: Don't allow deletion of 'admin' user
                if (user.Username.ToLower() == "admin")
                {
                    return Results.BadRequest(new { Message = "Utilizatorul 'admin' nu poate fi șters." });
                }

                // Protection: Don't allow self-deletion
                var currentUsername = principal.FindFirst(ClaimTypes.Name)?.Value;
                if (currentUsername != null && user.Username.Equals(currentUsername, StringComparison.OrdinalIgnoreCase))
                {
                    return Results.BadRequest(new { Message = "Nu vă puteți șterge propriul cont." });
                }

                db.Users.Remove(user);
                await db.SaveChangesAsync();
                return Results.NoContent();
            });

            return app;
        }
    }
}
