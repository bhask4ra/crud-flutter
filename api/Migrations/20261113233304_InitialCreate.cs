using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MedicosApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "catProductos",
                columns: table => new
                {
                    IdProducto = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Estatus = table.Column<bool>(type: "INTEGER", nullable: false),
                    Existencias = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catProductos", x => x.IdProducto);
                });

            migrationBuilder.InsertData(
                table: "catProductos",
                columns: new[] { "IdProducto", "Estatus", "Existencias", "Nombre", "PrecioUnitario", "Tipo" },
                values: new object[,]
                {
                    { 1, true, 8, "Laptop Dell Inspiron 15", 14500.00m, "Articulo" },
                    { 2, true, 3, "Mouse Inalámbrico Logitech", 450.00m, "Articulo" },
                    { 3, true, 0, "Mantenimiento Preventivo PC", 600.00m, "Servicio" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "catProductos");
        }
    }
}
