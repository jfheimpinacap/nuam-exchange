using System.ComponentModel.DataAnnotations;

namespace NuamExchange.Api.Contracts.Auth;

public sealed class LoginRequest
{
    [Required, EmailAddress, MaxLength(180)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public sealed record AuthUserResponse(int Id, string FullName, string Email, string Role);
public sealed record LoginResponse(string AccessToken, string TokenType, DateTime ExpiresAt, AuthUserResponse User);
public sealed record MeResponse(int Id, string FullName, string Email, string? JobTitle, string Role, bool IsActive);
public sealed record PermissionsResponse(IReadOnlyCollection<string> Permissions);
