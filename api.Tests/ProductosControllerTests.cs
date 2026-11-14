using Microsoft.AspNetCore.Mvc;
using MedicosApi.Controllers;
using MedicosApi.Dtos;
using MedicosApi.Models;
using Xunit;

namespace MedicosApi.Tests;

public class ProductosControllerTests
{
    private static ProductosController BuildController(params Producto[] seed) =>
        new(new FakeProductoRepository(seed));

    [Fact]
    public async Task GetAll_DevuelveTodos()
    {
        var ctrl = BuildController(
            new Producto { IdProducto = 1, Nombre = "Teclado", PrecioUnitario = 500, Existencias = 10, Tipo = TipoProducto.Articulo },
            new Producto { IdProducto = 2, Nombre = "Soporte Técnico", PrecioUnitario = 300, Existencias = 0, Tipo = TipoProducto.Servicio });

        var result = await ctrl.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<ProductoReadDto>>(ok.Value);
        Assert.Equal(2, list.Count());
    }

    [Fact]
    public async Task GetAll_FiltraPorSearchYTipo()
    {
        var ctrl = BuildController(
            new Producto { IdProducto = 1, Nombre = "Teclado Mecánico", PrecioUnitario = 800, Existencias = 4, Tipo = TipoProducto.Articulo },
            new Producto { IdProducto = 2, Nombre = "Limpieza de Teclado", PrecioUnitario = 200, Existencias = 0, Tipo = TipoProducto.Servicio });

        var result = await ctrl.GetAll(search: "teclado", tipo: TipoProducto.Servicio);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<ProductoReadDto>>(ok.Value).ToList();
        Assert.Single(list);
        Assert.Equal(2, list[0].IdProducto);
    }

    [Fact]
    public async Task GetById_Inexistente_DevuelveNotFound()
    {
        var ctrl = BuildController();
        var result = await ctrl.GetById(99);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ServicioFuerzaExistenciasEnCero()
    {
        var ctrl = BuildController();
        var dto = new ProductoCreateDto
        {
            Nombre = "Instalación",
            PrecioUnitario = 400,
            Existencias = 99,
            Tipo = TipoProducto.Servicio,
            Estatus = true
        };

        var result = await ctrl.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var read = Assert.IsType<ProductoReadDto>(created.Value);
        Assert.True(read.IdProducto > 0);
        Assert.Equal(0, read.Existencias);
        Assert.Equal(TipoProducto.Servicio, read.Tipo);
    }

    [Fact]
    public async Task Update_Existente_DevuelveNoContent()
    {
        var ctrl = BuildController(
            new Producto { IdProducto = 1, Nombre = "Mouse", PrecioUnitario = 250, Existencias = 5, Tipo = TipoProducto.Articulo });

        var dto = new ProductoUpdateDto
        {
            Nombre = "Mouse Pro",
            PrecioUnitario = 350,
            Existencias = 12,
            Tipo = TipoProducto.Articulo,
            Estatus = false
        };

        var result = await ctrl.Update(1, dto);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task AjustarExistencias_ArticuloValido_ActualizaStock()
    {
        var ctrl = BuildController(
            new Producto { IdProducto = 1, Nombre = "Monitor", PrecioUnitario = 3000, Existencias = 5, Tipo = TipoProducto.Articulo });

        var result = await ctrl.AjustarExistencias(1, new AjusteExistenciasDto { Delta = -2 });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<ProductoReadDto>(ok.Value);
        Assert.Equal(3, dto.Existencias);
    }

    [Fact]
    public async Task AjustarExistencias_Servicio_DevuelveBadRequest()
    {
        var ctrl = BuildController(
            new Producto { IdProducto = 1, Nombre = "Consultoría", PrecioUnitario = 900, Existencias = 0, Tipo = TipoProducto.Servicio });

        var result = await ctrl.AjustarExistencias(1, new AjusteExistenciasDto { Delta = 5 });
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task AjustarExistencias_NegativoResultante_DevuelveBadRequest()
    {
        var ctrl = BuildController(
            new Producto { IdProducto = 1, Nombre = "Cable HDMI", PrecioUnitario = 150, Existencias = 2, Tipo = TipoProducto.Articulo });

        var result = await ctrl.AjustarExistencias(1, new AjusteExistenciasDto { Delta = -5 });
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetResumen_CalculaTotalesCorrectamente()
    {
        var ctrl = BuildController(
            new Producto { IdProducto = 1, Nombre = "A", PrecioUnitario = 100, Existencias = 2, Tipo = TipoProducto.Articulo, Estatus = true },
            new Producto { IdProducto = 2, Nombre = "B", PrecioUnitario = 50, Existencias = 10, Tipo = TipoProducto.Articulo, Estatus = true },
            new Producto { IdProducto = 3, Nombre = "S", PrecioUnitario = 500, Existencias = 0, Tipo = TipoProducto.Servicio, Estatus = false });

        var result = await ctrl.GetResumen();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var resumen = Assert.IsType<ProductosResumenDto>(ok.Value);
        Assert.Equal(3, resumen.TotalProductos);
        Assert.Equal(2, resumen.Activos);
        Assert.Equal(1, resumen.Inactivos);
        Assert.Equal(2, resumen.TotalArticulos);
        Assert.Equal(1, resumen.TotalServicios);
        Assert.Equal(1, resumen.StockBajo);
        Assert.Equal(700m, resumen.ValorTotalInventario);
    }

    [Fact]
    public async Task Delete_Existente_DevuelveNoContent()
    {
        var ctrl = BuildController(
            new Producto { IdProducto = 1, Nombre = "A", PrecioUnitario = 10, Existencias = 1, Tipo = TipoProducto.Articulo });

        var result = await ctrl.Delete(1);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_Inexistente_DevuelveNotFound()
    {
        var ctrl = BuildController();
        var result = await ctrl.Delete(1);
        Assert.IsType<NotFoundResult>(result);
    }
}
