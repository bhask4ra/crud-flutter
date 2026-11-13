using Microsoft.AspNetCore.Mvc;
using MedicosApi.Controllers;
using MedicosApi.Dtos;
using MedicosApi.Models;
using Xunit;

namespace MedicosApi.Tests;

public class MedicosControllerTests
{
    private static MedicosController BuildController(params Medico[] seed) =>
        new(new FakeMedicoRepository(seed));

    [Fact]
    public async Task GetAll_DevuelveTodos()
    {
        var ctrl = BuildController(
            new Medico { IdMedico = 1, Nombre = "A", ApPaterno = "X", Especialidad = "E" },
            new Medico { IdMedico = 2, Nombre = "B", ApPaterno = "Y", Especialidad = "E" });

        var result = await ctrl.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<MedicoReadDto>>(ok.Value);
        Assert.Equal(2, list.Count());
    }

    [Fact]
    public async Task GetById_Inexistente_DevuelveNotFound()
    {
        var ctrl = BuildController();
        var result = await ctrl.GetById(99);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_DevuelveCreatedConId()
    {
        var ctrl = BuildController();
        var dto = new MedicoCreateDto
        {
            Nombre = "Nuevo", ApPaterno = "Medico", Especialidad = "Cardiología"
        };

        var result = await ctrl.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var read = Assert.IsType<MedicoReadDto>(created.Value);
        Assert.True(read.IdMedico > 0);
        Assert.Equal("Nuevo", read.Nombre);
    }

    [Fact]
    public async Task Update_Existente_DevuelveNoContent()
    {
        var ctrl = BuildController(
            new Medico { IdMedico = 1, Nombre = "A", ApPaterno = "X", Especialidad = "E" });

        var dto = new MedicoUpdateDto
        {
            Nombre = "Modificado", ApPaterno = "X", Especialidad = "E", Estatus = false
        };

        var result = await ctrl.Update(1, dto);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_Inexistente_DevuelveNotFound()
    {
        var ctrl = BuildController();
        var dto = new MedicoUpdateDto { Nombre = "X", ApPaterno = "Y", Especialidad = "E" };
        var result = await ctrl.Update(42, dto);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Existente_DevuelveNoContent()
    {
        var ctrl = BuildController(
            new Medico { IdMedico = 1, Nombre = "A", ApPaterno = "X", Especialidad = "E" });

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
