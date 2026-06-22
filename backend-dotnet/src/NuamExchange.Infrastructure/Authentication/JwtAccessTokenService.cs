using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace NuamExchange.Infrastructure.Authentication;

public sealed class JwtAccessTokenService(IOptions<JwtSettings> options, JwtConfigurationState configurationState) : IAccessTokenService
{
    private readonly JwtSettings _settings = options.Value;

    public AccessTokenResult CreateToken(AccessTokenUser user)
    {
        var configurationError = configurationState.GetValidationError();
        if (configurationError is not null)
        {
            throw new InvalidOperationException(configurationError);
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes > 0 ? _settings.AccessTokenMinutes : 30);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("name", user.FullName),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.RoleName),
            new("role", user.RoleName),
            new("role_id", user.RoleId.ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_settings.Issuer, _settings.Audience, claims, expires: expiresAt, signingCredentials: credentials);

        return new AccessTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
