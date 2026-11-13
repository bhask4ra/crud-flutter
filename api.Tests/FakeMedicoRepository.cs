using MedicosApi.Models;
using MedicosApi.Repositories;

namespace MedicosApi.Tests;

/// <summary>
/// Implementación en memoria de IMedicoRepository para probar el controller
/// de forma aislada, sin EF Core ni base de datos.
/// </summary>
public class FakeMedicoRepository : IMedicoRepository
{
    private readonly List<Medico> _items = new();
    private int _nextId = 1;

    public FakeMedicoRepository(IEnumerable<Medico>? seed = null)
    {
        if (seed is null) return;
        foreach (var m in seed)
        {
            if (m.IdMedico == 0) m.IdMedico = _nextId++;
            else _nextId = Math.Max(_nextId, m.IdMedico + 1);
            _items.Add(m);
        }
    }

    public Task<IEnumerable<Medico>> GetAllAsync() =>
        Task.FromResult(_items.OrderBy(m => m.IdMedico).AsEnumerable());

    public Task<Medico?> GetByIdAsync(int id) =>
        Task.FromResult(_items.FirstOrDefault(m => m.IdMedico == id));

    public Task<Medico> AddAsync(Medico medico)
    {
        medico.IdMedico = _nextId++;
        _items.Add(medico);
        return Task.FromResult(medico);
    }

    public Task<bool> UpdateAsync(Medico medico)
    {
        var idx = _items.FindIndex(m => m.IdMedico == medico.IdMedico);
        if (idx < 0) return Task.FromResult(false);
        _items[idx] = medico;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var removed = _items.RemoveAll(m => m.IdMedico == id);
        return Task.FromResult(removed > 0);
    }

    public Task<bool> ExistsAsync(int id) =>
        Task.FromResult(_items.Any(m => m.IdMedico == id));
}
