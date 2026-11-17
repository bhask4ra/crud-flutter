using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicosApi.Data;
using MedicosApi.Dtos;
using MedicosApi.Models;
using MedicosApi.Services;

namespace MedicosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtTokenService _jwt;

    public AuthController(AppDbContext db, JwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var exists = await _db.Usuarios.AnyAsync(u => u.Email == email);
        if (exists)
        {
            return Conflict(new { message = "El correo ya está registrado." });
        }

        var user = new Usuario
        {
            Nombre = dto.Nombre.Trim(),
            Email = email,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            FechaCreacion = DateTime.UtcNow
        };

        _db.Usuarios.Add(user);
        await _db.SaveChangesAsync();

        var (token, expiresAt) = _jwt.GenerateToken(user);
        return StatusCode(StatusCodes.Status201Created, new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Usuario = ToReadDto(user)
        });
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null || !PasswordHasher.Verify(dto.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Correo o contraseña incorrectos." });
        }

        var (token, expiresAt) = _jwt.GenerateToken(user);
        return Ok(new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Usuario = ToReadDto(user)
        });
    }

    // GET /api/auth/me
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UsuarioReadDto>> Me()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idStr, out var userId))
        {
            return Unauthorized();
        }

        var user = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == userId);
        if (user is null) return Unauthorized();

        return Ok(ToReadDto(user));
    }

    private static UsuarioReadDto ToReadDto(Usuario u) => new()
    {
        IdUsuario = u.IdUsuario,
        Nombre = u.Nombre,
        Email = u.Email
    };
}
