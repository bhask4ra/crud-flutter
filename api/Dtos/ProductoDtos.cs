using System.ComponentModel.DataAnnotations;
using MedicosApi.Models;

namespace MedicosApi.Dtos;

/// <summary>Datos de entrada para crear un producto.</summary>
public record ProductoCreateDto
{
    [Required, MaxLength(150)]
    public string Nombre { get; init; } = string.Empty;

    [Range(0, 999999999.99)]
    public decimal PrecioUnitario { get; init; }

    public bool Estatus { get; init; } = true;

    [Range(0, int.MaxValue)]
    public int Existencias { get; init; }

    [Required]
    public TipoProducto Tipo { get; init; } = TipoProducto.Articulo;
}

/// <summary>Datos de entrada para actualizar un producto.</summary>
public record ProductoUpdateDto
{
    [Required, MaxLength(150)]
    public string Nombre { get; init; } = string.Empty;

    [Range(0, 999999999.99)]
    public decimal PrecioUnitario { get; init; }

    public bool Estatus { get; init; } = true;

    [Range(0, int.MaxValue)]
    public int Existencias { get; init; }

    [Required]
    public TipoProducto Tipo { get; init; } = TipoProducto.Articulo;
}

/// <summary>Representación de salida de un producto.</summary>
public record ProductoReadDto
{
    public int IdProducto { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public decimal PrecioUnitario { get; init; }
    public bool Estatus { get; init; }
    public int Existencias { get; init; }
    public TipoProducto Tipo { get; init; }
}

/// <summary>Entrada para ajuste rápido de existencias (+ entrada / - salida).</summary>
public record AjusteExistenciasDto
{
    public int Delta { get; init; }
}

/// <summary>Resumen estadístico del catálogo de productos.</summary>
public record ProductosResumenDto
{
    public int TotalProductos { get; init; }
    public int Activos { get; init; }
    public int Inactivos { get; init; }
    public int TotalArticulos { get; init; }
    public int TotalServicios { get; init; }
    public int StockBajo { get; init; }
    public decimal ValorTotalInventario { get; init; }
}
