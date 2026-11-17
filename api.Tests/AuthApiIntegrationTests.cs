using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MedicosApi.Dtos;
using Xunit;

namespace MedicosApi.Tests;

public class AuthApiIntegrationTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);
    private readonly ApiFactory _factory;

    public AuthApiIntegrationTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Register_Login_Y_Me_FuncionanCorrectamente()
    {
        using var client = _factory.CreateClient();
        var email = $"user-{Guid.NewGuid():N}@ejemplo.com";

        // Register
        var regResp = await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Nombre = "Ana López",
            Email = email,
            Password = "MiPassword123"
        }, JsonOpts);
        Assert.Equal(HttpStatusCode.Created, regResp.StatusCode);

        var regBody = await regResp.Content.ReadFromJsonAsync<AuthResponseDto>(JsonOpts);
        Assert.NotNull(regBody);
        Assert.False(string.IsNullOrWhiteSpace(regBody!.Token));
        Assert.Equal("Ana López", regBody.Usuario.Nombre);
        Assert.Equal(email, regBody.Usuario.Email);

        // Duplicate register -> 409 Conflict
        var dupResp = await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Nombre = "Otra",
            Email = email.ToUpperInvariant(),
            Password = "MiPassword123"
        }, JsonOpts);
        Assert.Equal(HttpStatusCode.Conflict, dupResp.StatusCode);

        // Login with wrong password -> 401
        var badLogin = await client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = email,
            Password = "WrongPassword"
        }, JsonOpts);
        Assert.Equal(HttpStatusCode.Unauthorized, badLogin.StatusCode);

        // Login with valid credentials -> 200
        var loginResp = await client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = email,
            Password = "MiPassword123"
        }, JsonOpts);
        Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);

        var loginBody = await loginResp.Content.ReadFromJsonAsync<AuthResponseDto>(JsonOpts);
        Assert.NotNull(loginBody);
        Assert.False(string.IsNullOrWhiteSpace(loginBody!.Token));

        // GET /api/auth/me without token -> 401
        var meAnon = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meAnon.StatusCode);

        // GET /api/auth/me with token -> 200
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody.Token);
        var meResp = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResp.StatusCode);

        var meUser = await meResp.Content.ReadFromJsonAsync<UsuarioReadDto>(JsonOpts);
        Assert.Equal(email, meUser!.Email);
        Assert.Equal("Ana López", meUser.Nombre);
    }
}
