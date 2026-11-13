using System.ComponentModel.DataAnnotations;

namespace MedicosApi.Dtos;

/// <summary>Datos de entrada para crear un médico.</summary>
public record MedicoCreateDto
{
    [Required, MaxLength(100)]
    public string Nombre { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string ApPaterno { get; init; } = string.Empty;

    [MaxLength(100)]
    public string? ApMaterno { get; init; }

    [MaxLength(20)]
    public string? Telefono { get; init; }

    [MaxLength(200)]
    public string? Direccion { get; init; }

    [Required, MaxLength(100)]
    public string Especialidad { get; init; } = string.Empty;

    public bool Estatus { get; init; } = true;
}

/// <summary>Datos de entrada para actualizar un médico.</summary>
public record MedicoUpdateDto
{
    [Required, MaxLength(100)]
    public string Nombre { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string ApPaterno { get; init; } = string.Empty;

    [MaxLength(100)]
    public string? ApMaterno { get; init; }

    [MaxLength(20)]
    public string? Telefono { get; init; }

    [MaxLength(200)]
    public string? Direccion { get; init; }

    [Required, MaxLength(100)]
    public string Especialidad { get; init; } = string.Empty;

    public bool Estatus { get; init; } = true;
}

/// <summary>Representación de salida de un médico.</summary>
public record MedicoReadDto
{
    public int IdMedico { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string ApPaterno { get; init; } = string.Empty;
    public string? ApMaterno { get; init; }
    public string? Telefono { get; init; }
    public string? Direccion { get; init; }
    public string Especialidad { get; init; } = string.Empty;
    public bool Estatus { get; init; }
}
