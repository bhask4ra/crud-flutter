using Microsoft.EntityFrameworkCore;
using MedicosApi.Models;

namespace MedicosApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Producto> CatProductos => Set<Producto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producto>(e =>
        {
            e.ToTable("catProductos");
            e.HasKey(p => p.IdProducto);
            e.Property(p => p.Nombre).IsRequired().HasMaxLength(150);
            e.Property(p => p.PrecioUnitario).HasPrecision(18, 2);
            e.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(20);
        });

        // Datos de ejemplo (seed).
        modelBuilder.Entity<Producto>().HasData(
            new Producto
            {
                IdProducto = 1,
                Nombre = "Laptop Dell Inspiron 15",
                PrecioUnitario = 14500.00m,
                Estatus = true,
                Existencias = 8,
                Tipo = TipoProducto.Articulo
            },
            new Producto
            {
                IdProducto = 2,
                Nombre = "Mouse Inalámbrico Logitech",
                PrecioUnitario = 450.00m,
                Estatus = true,
                Existencias = 3,
                Tipo = TipoProducto.Articulo
            },
            new Producto
            {
                IdProducto = 3,
                Nombre = "Mantenimiento Preventivo PC",
                PrecioUnitario = 600.00m,
                Estatus = true,
                Existencias = 0,
                Tipo = TipoProducto.Servicio
            }
        );
    }
}
