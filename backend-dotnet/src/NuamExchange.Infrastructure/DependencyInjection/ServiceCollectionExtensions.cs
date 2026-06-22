using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NuamExchange.Infrastructure.Persistence;

namespace NuamExchange.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("NuamTributariaDb");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<NuamExchangeDbContext>(options => options.UseSqlServer(connectionString));
        }

        return services;
    }
}
