using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace MedicosApi.Services;

/// <summary>
/// Handler de autenticación Bearer que valida tokens JWT con JwtTokenService.
/// </summary>
public class JwtAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Bearer";
    private readonly JwtTokenService _jwtService;

    public JwtAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        JwtTokenService jwtService)
        : base(options, logger, encoder)
    {
        _jwtService = jwtService;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authHeader) ||
            !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var token = authHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrEmpty(token))
        {
            return Task.FromResult(AuthenticateResult.Fail("Token vacío."));
        }

        var principal = _jwtService.ValidateToken(token);
        if (principal is null)
        {
            return Task.FromResult(AuthenticateResult.Fail("Token inválido o expirado."));
        }

        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
