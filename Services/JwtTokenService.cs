using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace diet_tracker_api.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(string userId, string name, IEnumerable<string> permissions);
    string GenerateRefreshToken();
    string HashToken(string token);
    int AccessTokenExpiryMinutes { get; }
    int RefreshTokenExpiryDays { get; }
}

public class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    private readonly IConfiguration _configuration = configuration;

    public int AccessTokenExpiryMinutes =>
        int.TryParse(_configuration["Jwt:AccessTokenExpiryMinutes"], out var m) ? m : 15;

    public int RefreshTokenExpiryDays =>
        int.TryParse(_configuration["Jwt:RefreshTokenExpiryDays"], out var d) ? d : 7;

    public string GenerateAccessToken(string userId, string name, IEnumerable<string> permissions)
    {
        var secretKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");
        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(JwtRegisteredClaimNames.Name, name),
        };

        foreach (var permission in permissions)
        {
            // HasScopeHandler checks c.Value == permission && c.Issuer == issuer.
            // The JWT handler sets Claim.Issuer to the token's iss for all claims.
            claims.Add(new Claim("permissions", permission));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(AccessTokenExpiryMinutes),
            SigningCredentials = credentials,
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexStringLower(hash);
    }
}
