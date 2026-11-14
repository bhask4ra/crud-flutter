using MedicosApi.Dtos;
using MedicosApi.Models;

namespace MedicosApi.Repositories;

/// <summary>
/// Abstracción de persistencia de catProductos (DIP).
/// </summary>
public interface IProductoRepository
{
    Task<IEnumerable<Producto>> GetAllAsync(
        string? search = null,
        TipoProducto? tipo = null,
        bool? estatus = null);
    Task<Producto?> GetByIdAsync(int id);
    Task<Producto> AddAsync(Producto producto);
    Task<bool> UpdateAsync(Producto producto);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<(bool Found, string? Error, Producto? Producto)> AjustarExistenciasAsync(int id, int delta);
    Task<ProductosResumenDto> GetResumenAsync();
}
