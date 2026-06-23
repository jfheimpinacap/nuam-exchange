using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using NuamExchange.Api.Contracts.Administration;
using Xunit;

namespace NuamExchange.Api.Tests;

public sealed class CreateAdminRoleRequestContractTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Deserialize_WithTextDescription_BindsStringAndPermissionIds()
    {
        const string json = """
            {
              "name": "Revisor de Cargas",
              "description": "Rol de prueba para validar gestión controlada de permisos.",
              "permissionIds": [7, 8]
            }
            """;

        var request = JsonSerializer.Deserialize<CreateAdminRoleRequest>(json, JsonOptions);

        Assert.NotNull(request);
        Assert.Equal("Revisor de Cargas", request.Name);
        Assert.Equal("Rol de prueba para validar gestión controlada de permisos.", request.Description);
        Assert.Equal(new[] { 7, 8 }, request.PermissionIds);
    }

    [Fact]
    public void Deserialize_WithNullDescription_BindsNullDescription()
    {
        const string json = """
            {
              "name": "Revisor de Cargas",
              "description": null,
              "permissionIds": [7, 8]
            }
            """;

        var request = JsonSerializer.Deserialize<CreateAdminRoleRequest>(json, JsonOptions);

        Assert.NotNull(request);
        Assert.Equal("Revisor de Cargas", request.Name);
        Assert.Null(request.Description);
        Assert.Equal(new[] { 7, 8 }, request.PermissionIds);
    }

    [Fact]
    public void Contract_DescriptionIsOptionalNullableStringWithMaxLength()
    {
        var property = typeof(CreateAdminRoleRequest).GetProperty(nameof(CreateAdminRoleRequest.Description));

        Assert.NotNull(property);
        Assert.Equal(typeof(string), Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
        Assert.False(Attribute.IsDefined(property, typeof(RequiredAttribute)));
        var maxLength = Assert.Single(property.GetCustomAttributes(typeof(MaxLengthAttribute), inherit: false).Cast<MaxLengthAttribute>());
        Assert.Equal(250, maxLength.Length);
    }
}
