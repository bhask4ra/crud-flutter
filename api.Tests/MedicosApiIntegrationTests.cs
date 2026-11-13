using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MedicosApi.Data;
using MedicosApi.Dtos;
using Xunit;

namespace MedicosApi.Tests;

/// <summary>
/// Reemplaza el AppDbContext por una conexión SQLite en memoria (mantenida
/// abierta durante la vida de la factory) para probar la API de punta a punta,
/// incluyendo las migraciones y el seed, sin tocar el archivo real.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}

public class MedicosApiIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public MedicosApiIntegrationTests(ApiFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task GetAll_DevuelveSeedInicial()
    {
        var medicos = await _client.GetFromJsonAsync<List<MedicoReadDto>>("/api/medicos");
        Assert.NotNull(medicos);
        Assert.True(medicos!.Count >= 2);
    }

    [Fact]
    public async Task CicloCompleto_Crud()
    {
        // CREATE
        var nuevo = new MedicoCreateDto
        {
            Nombre = "Carla", ApPaterno = "Méndez", ApMaterno = "Soto",
            Telefono = "5550001111", Direccion = "Insurgentes 100",
            Especialidad = "Dermatología", Estatus = true
        };
        var postResp = await _client.PostAsJsonAsync("/api/medicos", nuevo);
        Assert.Equal(HttpStatusCode.Created, postResp.StatusCode);
        var creado = await postResp.Content.ReadFromJsonAsync<MedicoReadDto>();
        Assert.NotNull(creado);
        var id = creado!.IdMedico;
        Assert.True(id > 0);

        // READ by id
        var getResp = await _client.GetAsync($"/api/medicos/{id}");
        Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);

        // UPDATE
        var update = new MedicoUpdateDto
        {
            Nombre = "Carla", ApPaterno = "Méndez", ApMaterno = "Soto",
            Telefono = "5550001111", Direccion = "Insurgentes 100",
            Especialidad = "Dermatología Pediátrica", Estatus = false
        };
        var putResp = await _client.PutAsJsonAsync($"/api/medicos/{id}", update);
        Assert.Equal(HttpStatusCode.NoContent, putResp.StatusCode);

        var actualizado = await _client.GetFromJsonAsync<MedicoReadDto>($"/api/medicos/{id}");
        Assert.Equal("Dermatología Pediátrica", actualizado!.Especialidad);
        Assert.False(actualizado.Estatus);

        // DELETE
        var delResp = await _client.DeleteAsync($"/api/medicos/{id}");
        Assert.Equal(HttpStatusCode.NoContent, delResp.StatusCode);

        var getAfter = await _client.GetAsync($"/api/medicos/{id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfter.StatusCode);
    }

    [Fact]
    public async Task Post_SinCamposRequeridos_DevuelveBadRequest()
    {
        var invalido = new { Telefono = "123" }; // falta Nombre, ApPaterno, Especialidad
        var resp = await _client.PostAsJsonAsync("/api/medicos", invalido);
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }
}
