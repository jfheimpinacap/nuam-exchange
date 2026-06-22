using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using NuamExchange.Infrastructure.Authentication;

namespace NuamExchange.Api.Tests;

public sealed class AuthenticationTests
{
    [Fact]
    public void BcryptPasswordHasher_HashesAndVerifiesPassword()
    {
        const string password = "PasswordSeguro!123";
        var hasher = new BcryptPasswordHasher();

        var hash = hasher.Hash(password);

        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.NotEqual(password, hash);
        Assert.True(hasher.Verify(password, hash));
        Assert.False(hasher.Verify("PasswordIncorrecto!123", hash));
    }

    [Fact]
    public void JwtAccessTokenService_CreatesTokenWithExpectedClaims()
    {
        var settings = new JwtSettings
        {
            Issuer = "NuamExchange.Api.Tests",
            Audience = "NuamExchange.Tests",
            AccessTokenMinutes = 30,
            SigningKey = "TEST_SIGNING_KEY_WITH_AT_LEAST_32_CHARACTERS_123"
        };
        var options = Options.Create(settings);
        var service = new JwtAccessTokenService(options, new JwtConfigurationState(options));

        var result = service.CreateToken(new AccessTokenUser(7, "Usuario Prueba", "usuario@example.test", 1, "Administrador"));
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);

        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Contains(token.Claims, claim => claim.Type == JwtRegisteredClaimNames.Sub && claim.Value == "7");
        Assert.Contains(token.Claims, claim => claim.Type == JwtRegisteredClaimNames.Email && claim.Value == "usuario@example.test");
        Assert.Contains(token.Claims, claim => claim.Type == "role" && claim.Value == "Administrador");
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
        Assert.DoesNotContain(token.Claims, claim => claim.Type.Contains("password", StringComparison.OrdinalIgnoreCase) || claim.Value.Contains("hash", StringComparison.OrdinalIgnoreCase));
    }
}
