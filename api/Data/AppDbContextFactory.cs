using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MedicosApi.Data;

/// <summary>
/// Fábrica usada SOLO en tiempo de diseño por `dotnet ef` (migraciones).
/// Evita depender de que el host completo de la app arranque para generar
/// migraciones. No se usa en runtime.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=productos.db")
            .Options;
        return new AppDbContext(options);
    }
}
