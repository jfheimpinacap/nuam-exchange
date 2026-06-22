namespace NuamExchange.Infrastructure.Authentication;

public interface IAccessTokenService
{
    AccessTokenResult CreateToken(AccessTokenUser user);
}
