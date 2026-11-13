using System.ComponentModel.DataAnnotations;

namespace MedicosApi.Models;

/// <summary>
/// Entidad de dominio que representa a un médico.
/// </summary>
public class Medico
{
    public int IdMedico { get; set; }

    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string ApPaterno { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ApMaterno { get; set; }

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(200)]
    public string? Direccion { get; set; }

    [Required, MaxLength(100)]
    public string Especialidad { get; set; } = string.Empty;

    /// <summary>true = activo, false = inactivo.</summary>
    public bool Estatus { get; set; } = true;
}
