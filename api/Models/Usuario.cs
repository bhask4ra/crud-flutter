using System.ComponentModel.DataAnnotations;

namespace MedicosApi.Models;

/// <summary>
/// Entidad de dominio que representa un usuario registrado en el sistema.
/// </summary>
public class Usuario
{
    public int IdUsuario { get; set; }

    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
