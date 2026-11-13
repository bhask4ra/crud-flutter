using Microsoft.EntityFrameworkCore;
using MedicosApi.Models;

namespace MedicosApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Medico> Medicos => Set<Medico>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Medico>(e =>
        {
            e.HasKey(m => m.IdMedico);
            e.Property(m => m.Nombre).IsRequired().HasMaxLength(100);
            e.Property(m => m.ApPaterno).IsRequired().HasMaxLength(100);
            e.Property(m => m.ApMaterno).HasMaxLength(100);
            e.Property(m => m.Telefono).HasMaxLength(20);
            e.Property(m => m.Direccion).HasMaxLength(200);
            e.Property(m => m.Especialidad).IsRequired().HasMaxLength(100);
        });

        // Datos de ejemplo (seed).
        modelBuilder.Entity<Medico>().HasData(
            new Medico
            {
                IdMedico = 1, Nombre = "Ana", ApPaterno = "García", ApMaterno = "López",
                Telefono = "5551234567", Direccion = "Av. Reforma 123",
                Especialidad = "Cardiología", Estatus = true
            },
            new Medico
            {
                IdMedico = 2, Nombre = "Luis", ApPaterno = "Hernández", ApMaterno = "Ruiz",
                Telefono = "5559876543", Direccion = "Calle Juárez 45",
                Especialidad = "Pediatría", Estatus = true
            }
        );
    }
}
