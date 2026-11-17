using System.ComponentModel.DataAnnotations;

namespace MedicosApi.Dtos;

/// <summary>Datos de entrada para registrar un nuevo usuario.</summary>
public record RegisterDto
{
    [Required, MinLength(2), MaxLength(100)]
    public string Nombre { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; init; } = string.Empty;
}

/// <summary>Datos de entrada para iniciar sesión.</summary>
public record LoginDto
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

/// <summary>Información pública del usuario autenticado.</summary>
public record UsuarioReadDto
{
    public int IdUsuario { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

/// <summary>Respuesta de autenticación con token JWT y datos del usuario.</summary>
public record AuthResponseDto
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public UsuarioReadDto Usuario { get; init; } = new();
}
