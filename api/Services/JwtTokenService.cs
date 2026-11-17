using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MedicosApi.Models;

namespace MedicosApi.Services;

/// <summary>
/// Genera y valida tokens JWT firmados con HMAC-SHA256 (HS256).
/// </summary>
public class JwtTokenService
{
    private readonly byte[] _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expireMinutes;

    public JwtTokenService(IConfiguration configuration)
    {
        var secret = configuration["Jwt:Key"]
                     ?? "CatProductos-Dev-Secret-Key-Change-In-Prod-2026!";
        _key = Encoding.UTF8.GetBytes(secret);
        _issuer = configuration["Jwt:Issuer"] ?? "MedicosApi";
        _audience = configuration["Jwt:Audience"] ?? "CatProductosApp";
        _expireMinutes = int.TryParse(configuration["Jwt:ExpireMinutes"], out var mins) && mins > 0
            ? mins
            : 480;
    }

    public (string Token, DateTime ExpiresAt) GenerateToken(Usuario user)
    {
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(_expireMinutes);

        var headerJson = JsonSerializer.Serialize(new { alg = "HS256", typ = "JWT" });
        var payloadJson = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["sub"] = user.IdUsuario.ToString(),
            ["email"] = user.Email,
            ["name"] = user.Nombre,
            ["iss"] = _issuer,
            ["aud"] = _audience,
            ["iat"] = now.ToUnixTimeSeconds(),
            ["exp"] = expires.ToUnixTimeSeconds()
        });

        var headerB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
        var payloadB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        var signingInput = $"{headerB64}.{payloadB64}";
        var signature = ComputeHmac(signingInput);

        return ($"{signingInput}.{Base64UrlEncode(signature)}", expires.UtcDateTime);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3) return null;

        try
        {
            var headerBytes = Base64UrlDecode(parts[0]);
            using var headerDoc = JsonDocument.Parse(headerBytes);
            if (!headerDoc.RootElement.TryGetProperty("alg", out var algEl) ||
                algEl.GetString() != "HS256")
            {
                return null;
            }

            var signingInput = $"{parts[0]}.{parts[1]}";
            var expectedSig = ComputeHmac(signingInput);
            var actualSig = Base64UrlDecode(parts[2]);

            if (!CryptographicOperations.FixedTimeEquals(expectedSig, actualSig))
            {
                return null;
            }

            var payloadBytes = Base64UrlDecode(parts[1]);
            using var payloadDoc = JsonDocument.Parse(payloadBytes);
            var root = payloadDoc.RootElement;

            if (!root.TryGetProperty("iss", out var issEl) || issEl.GetString() != _issuer)
                return null;
            if (!root.TryGetProperty("aud", out var audEl) || audEl.GetString() != _audience)
                return null;
            if (!root.TryGetProperty("exp", out var expEl) || !expEl.TryGetInt64(out var expUnix))
                return null;

            var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (nowUnix >= expUnix) return null;

            var sub = root.TryGetProperty("sub", out var subEl) ? subEl.GetString() : null;
            var email = root.TryGetProperty("email", out var emailEl) ? emailEl.GetString() : null;
            var name = root.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null;

            if (string.IsNullOrEmpty(sub) || string.IsNullOrEmpty(email))
                return null;

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, sub),
                new(ClaimTypes.Email, email),
                new(ClaimTypes.Name, name ?? email)
            };

            var identity = new ClaimsIdentity(claims, "Bearer");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return null;
        }
    }

    private byte[] ComputeHmac(string input)
    {
        using var hmac = new HMACSHA256(_key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
    }

    private static string Base64UrlEncode(byte[] data) =>
        Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static byte[] Base64UrlDecode(string input)
    {
        var padded = input.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        return Convert.FromBase64String(padded);
    }
}
