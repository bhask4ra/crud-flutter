using Microsoft.AspNetCore.Mvc;
using MedicosApi.Dtos;
using MedicosApi.Models;
using MedicosApi.Repositories;

namespace MedicosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicosController : ControllerBase
{
    private readonly IMedicoRepository _repo;

    public MedicosController(IMedicoRepository repo) => _repo = repo;

    // GET /api/medicos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MedicoReadDto>>> GetAll()
    {
        var medicos = await _repo.GetAllAsync();
        return Ok(medicos.Select(ToReadDto));
    }

    // GET /api/medicos/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MedicoReadDto>> GetById(int id)
    {
        var medico = await _repo.GetByIdAsync(id);
        if (medico is null) return NotFound();
        return Ok(ToReadDto(medico));
    }

    // POST /api/medicos
    [HttpPost]
    public async Task<ActionResult<MedicoReadDto>> Create([FromBody] MedicoCreateDto dto)
    {
        var medico = new Medico
        {
            Nombre = dto.Nombre,
            ApPaterno = dto.ApPaterno,
            ApMaterno = dto.ApMaterno,
            Telefono = dto.Telefono,
            Direccion = dto.Direccion,
            Especialidad = dto.Especialidad,
            Estatus = dto.Estatus
        };

        var creado = await _repo.AddAsync(medico);
        return CreatedAtAction(nameof(GetById),
            new { id = creado.IdMedico }, ToReadDto(creado));
    }

    // PUT /api/medicos/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] MedicoUpdateDto dto)
    {
        if (!await _repo.ExistsAsync(id)) return NotFound();

        var medico = new Medico
        {
            IdMedico = id,
            Nombre = dto.Nombre,
            ApPaterno = dto.ApPaterno,
            ApMaterno = dto.ApMaterno,
            Telefono = dto.Telefono,
            Direccion = dto.Direccion,
            Especialidad = dto.Especialidad,
            Estatus = dto.Estatus
        };

        var ok = await _repo.UpdateAsync(medico);
        if (!ok) return Conflict("No se pudo actualizar el médico.");
        return NoContent();
    }

    // DELETE /api/medicos/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }

    private static MedicoReadDto ToReadDto(Medico m) => new()
    {
        IdMedico = m.IdMedico,
        Nombre = m.Nombre,
        ApPaterno = m.ApPaterno,
        ApMaterno = m.ApMaterno,
        Telefono = m.Telefono,
        Direccion = m.Direccion,
        Especialidad = m.Especialidad,
        Estatus = m.Estatus
    };
}
