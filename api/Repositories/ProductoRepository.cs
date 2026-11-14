using Microsoft.EntityFrameworkCore;
using MedicosApi.Data;
using MedicosApi.Dtos;
using MedicosApi.Models;

namespace MedicosApi.Repositories;

public class ProductoRepository : IProductoRepository
{
    private const int UmbralStockBajo = 5;
    private readonly AppDbContext _db;

    public ProductoRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Producto>> GetAllAsync(
        string? search = null,
        TipoProducto? tipo = null,
        bool? estatus = null)
    {
        var query = _db.CatProductos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p => p.Nombre.ToLower().Contains(term));
        }

        if (tipo.HasValue)
            query = query.Where(p => p.Tipo == tipo.Value);

        if (estatus.HasValue)
            query = query.Where(p => p.Estatus == estatus.Value);

        return await query.OrderBy(p => p.IdProducto).ToListAsync();
    }

    public async Task<Producto?> GetByIdAsync(int id) =>
        await _db.CatProductos.AsNoTracking().FirstOrDefaultAsync(p => p.IdProducto == id);

    public async Task<Producto> AddAsync(Producto producto)
    {
        if (producto.Tipo == TipoProducto.Servicio)
            producto.Existencias = 0;

        _db.CatProductos.Add(producto);
        await _db.SaveChangesAsync();
        return producto;
    }

    public async Task<bool> UpdateAsync(Producto producto)
    {
        if (producto.Tipo == TipoProducto.Servicio)
            producto.Existencias = 0;

        _db.CatProductos.Update(producto);
        try
        {
            await _db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var producto = await _db.CatProductos.FindAsync(id);
        if (producto is null) return false;
        _db.CatProductos.Remove(producto);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _db.CatProductos.AnyAsync(p => p.IdProducto == id);

    public async Task<(bool Found, string? Error, Producto? Producto)> AjustarExistenciasAsync(int id, int delta)
    {
        var producto = await _db.CatProductos.FindAsync(id);
        if (producto is null) return (false, null, null);

        if (producto.Tipo == TipoProducto.Servicio)
            return (true, "Los servicios no son inventariables; no se pueden ajustar existencias.", null);

        var nuevasExistencias = producto.Existencias + delta;
        if (nuevasExistencias < 0)
            return (true, "Las existencias no pueden quedar en negativo.", null);

        producto.Existencias = nuevasExistencias;
        await _db.SaveChangesAsync();
        return (true, null, producto);
    }

    public async Task<ProductosResumenDto> GetResumenAsync()
    {
        var items = await _db.CatProductos.AsNoTracking().ToListAsync();
        return new ProductosResumenDto
        {
            TotalProductos = items.Count,
            Activos = items.Count(p => p.Estatus),
            Inactivos = items.Count(p => !p.Estatus),
            TotalArticulos = items.Count(p => p.Tipo == TipoProducto.Articulo),
            TotalServicios = items.Count(p => p.Tipo == TipoProducto.Servicio),
            StockBajo = items.Count(p => p.Tipo == TipoProducto.Articulo && p.Estatus && p.Existencias < UmbralStockBajo),
            ValorTotalInventario = items
                .Where(p => p.Tipo == TipoProducto.Articulo && p.Estatus)
                .Sum(p => p.PrecioUnitario * p.Existencias)
        };
    }
}
