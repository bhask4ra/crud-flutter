using MedicosApi.Dtos;
using MedicosApi.Models;
using MedicosApi.Repositories;

namespace MedicosApi.Tests;

/// <summary>
/// Implementación en memoria de IProductoRepository para probar el controller
/// de forma aislada, sin EF Core ni base de datos.
/// </summary>
public class FakeProductoRepository : IProductoRepository
{
    private const int UmbralStockBajo = 5;
    private readonly List<Producto> _items = new();
    private int _nextId = 1;

    public FakeProductoRepository(IEnumerable<Producto>? seed = null)
    {
        if (seed is null) return;
        foreach (var p in seed)
        {
            if (p.IdProducto == 0) p.IdProducto = _nextId++;
            else _nextId = Math.Max(_nextId, p.IdProducto + 1);
            _items.Add(p);
        }
    }

    public Task<IEnumerable<Producto>> GetAllAsync(
        string? search = null,
        TipoProducto? tipo = null,
        bool? estatus = null)
    {
        var query = _items.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(p => p.Nombre.ToLowerInvariant().Contains(term));
        }
        if (tipo.HasValue)
            query = query.Where(p => p.Tipo == tipo.Value);
        if (estatus.HasValue)
            query = query.Where(p => p.Estatus == estatus.Value);

        return Task.FromResult(query.OrderBy(p => p.IdProducto).AsEnumerable());
    }

    public Task<Producto?> GetByIdAsync(int id) =>
        Task.FromResult(_items.FirstOrDefault(p => p.IdProducto == id));

    public Task<Producto> AddAsync(Producto producto)
    {
        if (producto.Tipo == TipoProducto.Servicio)
            producto.Existencias = 0;

        producto.IdProducto = _nextId++;
        _items.Add(producto);
        return Task.FromResult(producto);
    }

    public Task<bool> UpdateAsync(Producto producto)
    {
        if (producto.Tipo == TipoProducto.Servicio)
            producto.Existencias = 0;

        var idx = _items.FindIndex(p => p.IdProducto == producto.IdProducto);
        if (idx < 0) return Task.FromResult(false);
        _items[idx] = producto;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var removed = _items.RemoveAll(p => p.IdProducto == id);
        return Task.FromResult(removed > 0);
    }

    public Task<bool> ExistsAsync(int id) =>
        Task.FromResult(_items.Any(p => p.IdProducto == id));

    public Task<(bool Found, string? Error, Producto? Producto)> AjustarExistenciasAsync(int id, int delta)
    {
        var producto = _items.FirstOrDefault(p => p.IdProducto == id);
        if (producto is null)
            return Task.FromResult<(bool, string?, Producto?)>((false, null, null));

        if (producto.Tipo == TipoProducto.Servicio)
            return Task.FromResult<(bool, string?, Producto?)>(
                (true, "Los servicios no son inventariables; no se pueden ajustar existencias.", null));

        var nuevo = producto.Existencias + delta;
        if (nuevo < 0)
            return Task.FromResult<(bool, string?, Producto?)>(
                (true, "Las existencias no pueden quedar en negativo.", null));

        producto.Existencias = nuevo;
        return Task.FromResult<(bool, string?, Producto?)>((true, null, producto));
    }

    public Task<ProductosResumenDto> GetResumenAsync()
    {
        return Task.FromResult(new ProductosResumenDto
        {
            TotalProductos = _items.Count,
            Activos = _items.Count(p => p.Estatus),
            Inactivos = _items.Count(p => !p.Estatus),
            TotalArticulos = _items.Count(p => p.Tipo == TipoProducto.Articulo),
            TotalServicios = _items.Count(p => p.Tipo == TipoProducto.Servicio),
            StockBajo = _items.Count(p => p.Tipo == TipoProducto.Articulo && p.Estatus && p.Existencias < UmbralStockBajo),
            ValorTotalInventario = _items
                .Where(p => p.Tipo == TipoProducto.Articulo && p.Estatus)
                .Sum(p => p.PrecioUnitario * p.Existencias)
        });
    }
}
