using System.ComponentModel.DataAnnotations;

namespace NuamExchange.Api.Contracts.Setup;

public sealed class BootstrapAdminRequest
{
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(180)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? JobTitle { get; set; }
}

public sealed record BootstrapAdminResponse(int Id, string FullName, string Email, string Role, string Message);
