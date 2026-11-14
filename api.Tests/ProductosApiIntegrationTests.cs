using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MedicosApi.Data;
using MedicosApi.Dtos;
using MedicosApi.Models;
using Xunit;

namespace MedicosApi.Tests;

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

public class ProductosApiIntegrationTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client;

    public ProductosApiIntegrationTests(ApiFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task GetAll_DevuelveSeedInicial()
    {
        var productos = await _client.GetFromJsonAsync<List<ProductoReadDto>>("/api/productos", JsonOpts);
        Assert.NotNull(productos);
        Assert.True(productos!.Count >= 3);
    }

    [Fact]
    public async Task CicloCompleto_CrudYAjusteExistencias()
    {
        // CREATE
        var nuevo = new ProductoCreateDto
        {
            Nombre = "Teclado RGB",
            PrecioUnitario = 1250.50m,
            Estatus = true,
            Existencias = 10,
            Tipo = TipoProducto.Articulo
        };
        var postResp = await _client.PostAsJsonAsync("/api/productos", nuevo, JsonOpts);
        Assert.Equal(HttpStatusCode.Created, postResp.StatusCode);
        var creado = await postResp.Content.ReadFromJsonAsync<ProductoReadDto>(JsonOpts);
        Assert.NotNull(creado);
        var id = creado!.IdProducto;
        Assert.True(id > 0);

        // PATCH existencias (+5)
        var patchResp = await _client.PatchAsJsonAsync(
            $"/api/productos/{id}/existencias",
            new AjusteExistenciasDto { Delta = 5 },
            JsonOpts);
        Assert.Equal(HttpStatusCode.OK, patchResp.StatusCode);
        var ajustado = await patchResp.Content.ReadFromJsonAsync<ProductoReadDto>(JsonOpts);
        Assert.Equal(15, ajustado!.Existencias);

        // READ by id
        var getResp = await _client.GetAsync($"/api/productos/{id}");
        Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);

        // UPDATE
        var update = new ProductoUpdateDto
        {
            Nombre = "Teclado RGB Pro",
            PrecioUnitario = 1400.00m,
            Estatus = false,
            Existencias = 15,
            Tipo = TipoProducto.Articulo
        };
        var putResp = await _client.PutAsJsonAsync($"/api/productos/{id}", update, JsonOpts);
        Assert.Equal(HttpStatusCode.NoContent, putResp.StatusCode);

        var actualizado = await _client.GetFromJsonAsync<ProductoReadDto>($"/api/productos/{id}", JsonOpts);
        Assert.Equal("Teclado RGB Pro", actualizado!.Nombre);
        Assert.False(actualizado.Estatus);

        // DELETE
        var delResp = await _client.DeleteAsync($"/api/productos/{id}");
        Assert.Equal(HttpStatusCode.NoContent, delResp.StatusCode);

        var getAfter = await _client.GetAsync($"/api/productos/{id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfter.StatusCode);
    }

    [Fact]
    public async Task GetResumen_DevuelveOk()
    {
        var resumen = await _client.GetFromJsonAsync<ProductosResumenDto>("/api/productos/resumen", JsonOpts);
        Assert.NotNull(resumen);
        Assert.True(resumen!.TotalProductos >= 3);
    }

    [Fact]
    public async Task Post_SinNombre_DevuelveBadRequest()
    {
        var invalido = new { PrecioUnitario = 100, Tipo = "Articulo" };
        var resp = await _client.PostAsJsonAsync("/api/productos", invalido);
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }
}
