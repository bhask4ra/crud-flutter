using Microsoft.EntityFrameworkCore;
using MedicosApi.Data;
using MedicosApi.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Kestrel en el puerto 5000 (accesible desde emulador/dispositivo).
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// EF Core + SQLite.
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")
                  ?? "Data Source=medicos.db"));

builder.Services.AddScoped<IMedicoRepository, MedicoRepository>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// CORS abierto para el cliente móvil.
const string CorsPolicy = "AllowAll";
builder.Services.AddCors(opt =>
    opt.AddPolicy(CorsPolicy, p => p.AllowAnyOrigin()
                                    .AllowAnyHeader()
                                    .AllowAnyMethod()));

var app = builder.Build();

// Aplica migraciones automáticamente al arrancar (crea la BD si no existe).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    // Documento OpenAPI en http://localhost:5000/openapi/v1.json
    app.MapOpenApi();
}

app.UseCors(CorsPolicy);
app.MapControllers();

app.Run();

// Expuesto para los tests de integración (WebApplicationFactory<Program>).
public partial class Program { }
