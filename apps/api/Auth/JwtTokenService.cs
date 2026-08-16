using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ModelaFlow.Api.Domain.Identity;

namespace ModelaFlow.Api.Auth;

public sealed class JwtTokenService
{
    private readonly AuthOptions _options;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtTokenService(IOptions<AuthOptions> options)
    {
        _options = options.Value;
        if (string.IsNullOrWhiteSpace(_options.JwtSigningKey) || _options.JwtSigningKey.Length < 32)
            throw new InvalidOperationException("Auth:JwtSigningKey must be at least 32 characters.");

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.JwtSigningKey));
    }

    public string CreateToken(User user, string organizationName)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString("D")),
            new(AuthClaims.UserId, user.Id.ToString("D")),
            new(AuthClaims.TenantId, user.TenantId.ToString("D")),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("organization_name", organizationName),
            new("security_stamp", user.SecurityStamp)
        };

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_options.TokenLifetimeMinutes);

        var token = new JwtSecurityToken(
            issuer: _options.JwtIssuer,
            audience: _options.JwtAudience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public TokenValidationParameters CreateValidationParameters() =>
        new()
        {
            ValidateIssuer = true,
            ValidIssuer = _options.JwtIssuer,
            ValidateAudience = true,
            ValidAudience = _options.JwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = ClaimTypes.Role
        };
}
