using Microsoft.EntityFrameworkCore;
using MedicosApi.Data;
using MedicosApi.Models;

namespace MedicosApi.Repositories;

public class MedicoRepository : IMedicoRepository
{
    private readonly AppDbContext _db;

    public MedicoRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Medico>> GetAllAsync() =>
        await _db.Medicos.AsNoTracking().OrderBy(m => m.IdMedico).ToListAsync();

    public async Task<Medico?> GetByIdAsync(int id) =>
        await _db.Medicos.AsNoTracking().FirstOrDefaultAsync(m => m.IdMedico == id);

    public async Task<Medico> AddAsync(Medico medico)
    {
        _db.Medicos.Add(medico);
        await _db.SaveChangesAsync();
        return medico;
    }

    public async Task<bool> UpdateAsync(Medico medico)
    {
        _db.Medicos.Update(medico);
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
        var medico = await _db.Medicos.FindAsync(id);
        if (medico is null) return false;
        _db.Medicos.Remove(medico);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _db.Medicos.AnyAsync(m => m.IdMedico == id);
}
