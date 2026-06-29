using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NuamExchange.Infrastructure.Persistence;

namespace NuamExchange.Api.Tests;

internal static class TestInMemoryDatabase
{
    public static string CreateDatabaseName() => $"NuamExchange.Api.Tests.{Guid.NewGuid():N}";

    public static IServiceCollection AddNuamExchangeInMemoryDatabase(this IServiceCollection services, string? databaseName = null)
    {
        services.RemoveAll<DbContextOptions<NuamExchangeDbContext>>();
        services.RemoveAll<NuamExchangeDbContext>();

        var effectiveDatabaseName = string.IsNullOrWhiteSpace(databaseName) ? CreateDatabaseName() : databaseName;
        services.AddDbContext<NuamExchangeDbContext>(options => options
            .UseInMemoryDatabase(effectiveDatabaseName)
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

        return services;
    }
}
