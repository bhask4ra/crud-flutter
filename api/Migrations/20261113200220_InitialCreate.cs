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
                name: "Medicos",
                columns: table => new
                {
                    IdMedico = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ApPaterno = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ApMaterno = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Direccion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Especialidad = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Estatus = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicos", x => x.IdMedico);
                });

            migrationBuilder.InsertData(
                table: "Medicos",
                columns: new[] { "IdMedico", "ApMaterno", "ApPaterno", "Direccion", "Especialidad", "Estatus", "Nombre", "Telefono" },
                values: new object[,]
                {
                    { 1, "López", "García", "Av. Reforma 123", "Cardiología", true, "Ana", "5551234567" },
                    { 2, "Ruiz", "Hernández", "Calle Juárez 45", "Pediatría", true, "Luis", "5559876543" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Medicos");
        }
    }
}
