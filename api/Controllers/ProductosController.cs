using Microsoft.AspNetCore.Mvc;
using MedicosApi.Dtos;
using MedicosApi.Models;
using MedicosApi.Repositories;

namespace MedicosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IProductoRepository _repo;

    public ProductosController(IProductoRepository repo) => _repo = repo;

    // GET /api/productos?search=...&tipo=Articulo&estatus=true
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoReadDto>>> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] TipoProducto? tipo = null,
        [FromQuery] bool? estatus = null)
    {
        var productos = await _repo.GetAllAsync(search, tipo, estatus);
        return Ok(productos.Select(ToReadDto));
    }

    // GET /api/productos/resumen
    [HttpGet("resumen")]
    public async Task<ActionResult<ProductosResumenDto>> GetResumen()
    {
        var resumen = await _repo.GetResumenAsync();
        return Ok(resumen);
    }

    // GET /api/productos/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoReadDto>> GetById(int id)
    {
        var producto = await _repo.GetByIdAsync(id);
        if (producto is null) return NotFound();
        return Ok(ToReadDto(producto));
    }

    // POST /api/productos
    [HttpPost]
    public async Task<ActionResult<ProductoReadDto>> Create([FromBody] ProductoCreateDto dto)
    {
        var producto = new Producto
        {
            Nombre = dto.Nombre.Trim(),
            PrecioUnitario = dto.PrecioUnitario,
            Estatus = dto.Estatus,
            Existencias = dto.Tipo == TipoProducto.Servicio ? 0 : dto.Existencias,
            Tipo = dto.Tipo
        };

        var creado = await _repo.AddAsync(producto);
        return CreatedAtAction(nameof(GetById),
            new { id = creado.IdProducto }, ToReadDto(creado));
    }

    // PUT /api/productos/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductoUpdateDto dto)
    {
        if (!await _repo.ExistsAsync(id)) return NotFound();

        var producto = new Producto
        {
            IdProducto = id,
            Nombre = dto.Nombre.Trim(),
            PrecioUnitario = dto.PrecioUnitario,
            Estatus = dto.Estatus,
            Existencias = dto.Tipo == TipoProducto.Servicio ? 0 : dto.Existencias,
            Tipo = dto.Tipo
        };

        var ok = await _repo.UpdateAsync(producto);
        if (!ok) return Conflict("No se pudo actualizar el producto.");
        return NoContent();
    }

    // PATCH /api/productos/5/existencias
    [HttpPatch("{id:int}/existencias")]
    public async Task<ActionResult<ProductoReadDto>> AjustarExistencias(
        int id,
        [FromBody] AjusteExistenciasDto dto)
    {
        var (found, error, producto) = await _repo.AjustarExistenciasAsync(id, dto.Delta);
        if (!found) return NotFound();
        if (error is not null) return BadRequest(new { message = error });
        return Ok(ToReadDto(producto!));
    }

    // DELETE /api/productos/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }

    private static ProductoReadDto ToReadDto(Producto p) => new()
    {
        IdProducto = p.IdProducto,
        Nombre = p.Nombre,
        PrecioUnitario = p.PrecioUnitario,
        Estatus = p.Estatus,
        Existencias = p.Existencias,
        Tipo = p.Tipo
    };
}
