using MedicosApi.Models;

namespace MedicosApi.Repositories;

/// <summary>
/// Abstracción de persistencia de médicos (DIP): la capa web depende de esta
/// interfaz, no de EF Core, lo que permite tests sin base de datos real y
/// cambiar el proveedor (SQLite → SQL Server) sin tocar los controllers.
/// </summary>
public interface IMedicoRepository
{
    Task<IEnumerable<Medico>> GetAllAsync();
    Task<Medico?> GetByIdAsync(int id);
    Task<Medico> AddAsync(Medico medico);
    Task<bool> UpdateAsync(Medico medico);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
