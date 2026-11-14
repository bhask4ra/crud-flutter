using System.ComponentModel.DataAnnotations;

namespace MedicosApi.Models;

/// <summary>
/// Clasificación del producto dentro del catálogo.
/// </summary>
public enum TipoProducto
{
    Articulo = 1,
    Servicio = 2
}

/// <summary>
/// Entidad de dominio que representa un registro de catProductos.
/// </summary>
public class Producto
{
    public int IdProducto { get; set; }

    [Required, MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Range(0, 999999999.99)]
    public decimal PrecioUnitario { get; set; }

    /// <summary>true = activo, false = inactivo.</summary>
    public bool Estatus { get; set; } = true;

    [Range(0, int.MaxValue)]
    public int Existencias { get; set; }

    public TipoProducto Tipo { get; set; } = TipoProducto.Articulo;
}
