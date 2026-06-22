namespace NuamExchange.Infrastructure.Authentication;

public sealed record AccessTokenUser(int Id, string FullName, string Email, int RoleId, string RoleName);

public sealed record AccessTokenResult(string Token, DateTime ExpiresAt);
