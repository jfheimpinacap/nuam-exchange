using System.ComponentModel.DataAnnotations;

namespace NuamExchange.Api.Contracts.Administration;

public sealed class CreateAdminUserRequest
{
    [Required, MaxLength(150)] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(180)] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
    [MaxLength(120)] public string? JobTitle { get; set; }
    [Required] public int RoleId { get; set; }
}

public sealed class UpdateAdminUserRequest
{
    [Required, MaxLength(150)] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(180)] public string Email { get; set; } = string.Empty;
    [MaxLength(120)] public string? JobTitle { get; set; }
    [Required] public int RoleId { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ResetAdminUserPasswordRequest
{
    [Required] public string NewPassword { get; set; } = string.Empty;
}
