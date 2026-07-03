using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuamExchange.Application.Administration;
using Xunit;

namespace NuamExchange.Api.Tests;

public sealed class AdminRolesEndpointBindingTests
{
    [Fact]
    public async Task PostRoles_WithPublishedJsonContract_DoesNotFailModelBinding()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddNuamExchangeInMemoryDatabase();
                services.RemoveAll<IAdministrationService>();
                services.AddScoped<IAdministrationService, CapturingAdministrationService>();
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthenticationHandler.AuthenticationScheme;
                    options.DefaultChallengeScheme = TestAuthenticationHandler.AuthenticationScheme;
                }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.AuthenticationScheme, _ => { });
            });
        });
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/admin/roles", new
        {
            name = "Revisor de Cargas",
            description = "Rol de prueba para validar gestión controlada de permisos.",
            permissionIds = new[] { 7, 8 }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var role = await response.Content.ReadFromJsonAsync<AdminRoleDetailResponse>();
        Assert.NotNull(role);
        Assert.Equal("Rol de prueba para validar gestión controlada de permisos.", role.Description);
    }

    private sealed class CapturingAdministrationService : IAdministrationService
    {
        public Task<AdministrationResult<AdminRoleDetailResponse>> CreateRoleAsync(CreateRoleCommand command, int administratorId, string? originIp, CancellationToken cancellationToken)
        {
            var response = new AdminRoleDetailResponse(99, command.Name, command.Description, true, false, command.PermissionIds!.Select(id => new AdminPermissionResponse(id, $"permission.{id}", null)).ToList());
            return Task.FromResult(AdministrationResult<AdminRoleDetailResponse>.Created(response));
        }

        public Task<AdministrationResult<AdminRoleDetailResponse>> GetRoleAsync(int id, CancellationToken cancellationToken) => Task.FromResult(AdministrationResult<AdminRoleDetailResponse>.Fail(404, "El rol indicado no existe."));
        public Task<IReadOnlyCollection<AdminRoleResponse>> GetRolesAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<AdminRoleResponse>>([]);
        public Task<IReadOnlyCollection<AdminPermissionResponse>> GetPermissionsAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<AdminPermissionResponse>>([]);
        public Task<AdministrationResult<PagedResponse<AdminUserResponse>>> GetUsersAsync(string? search, int? roleId, bool? isActive, int page, int pageSize, CancellationToken cancellationToken) => Task.FromResult(AdministrationResult<PagedResponse<AdminUserResponse>>.Fail(404, "No implementado."));
        public Task<AdministrationResult<AdminUserResponse>> GetUserAsync(int id, CancellationToken cancellationToken) => Task.FromResult(AdministrationResult<AdminUserResponse>.Fail(404, "No implementado."));
        public Task<AdministrationResult<AdminUserResponse>> CreateUserAsync(string fullName, string email, string password, string? jobTitle, int roleId, bool isActive, int administratorId, string? originIp, CancellationToken cancellationToken) => Task.FromResult(AdministrationResult<AdminUserResponse>.Fail(404, "No implementado."));
        public Task<AdministrationResult<AdminUserResponse>> UpdateUserAsync(int id, string fullName, string email, string? jobTitle, int roleId, bool isActive, int administratorId, string? originIp, CancellationToken cancellationToken) => Task.FromResult(AdministrationResult<AdminUserResponse>.Fail(404, "No implementado."));
        public Task<AdministrationResult> ResetPasswordAsync(int id, string newPassword, int administratorId, string? originIp, CancellationToken cancellationToken) => Task.FromResult(AdministrationResult.Fail(404, "No implementado."));
        public Task<AdministrationResult<AdminRoleDetailResponse>> UpdateRoleAsync(int id, UpdateRoleCommand command, int administratorId, string? originIp, CancellationToken cancellationToken) => Task.FromResult(AdministrationResult<AdminRoleDetailResponse>.Fail(404, "No implementado."));
        public Task<AdministrationResult<AdminRoleDetailResponse>> UpdateRolePermissionsAsync(int id, UpdateRolePermissionsCommand command, int administratorId, string? originIp, CancellationToken cancellationToken) => Task.FromResult(AdministrationResult<AdminRoleDetailResponse>.Fail(404, "No implementado."));
    }

    private sealed class TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string AuthenticationScheme = "Test";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] { new Claim("sub", "1"), new Claim(ClaimTypes.Role, "Administrador") };
            var identity = new ClaimsIdentity(claims, AuthenticationScheme);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), AuthenticationScheme)));
        }
    }
}
