using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NuamExchange.Infrastructure.Persistence;

namespace NuamExchange.Api.Persistence;

public sealed class NuamExchangeDbContextFactory : IDesignTimeDbContextFactory<NuamExchangeDbContext>
{
    private const string ApiProjectRelativePath = "src/NuamExchange.Api";

    public NuamExchangeDbContext CreateDbContext(string[] args)
    {
        var apiProjectPath = ResolveApiProjectPath();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("NuamTributariaDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No se encontró la cadena de conexión 'ConnectionStrings:NuamTributariaDb'. " +
                "Cree localmente el archivo ignorado por Git 'appsettings.Development.json' en " +
                "'backend-dotnet/src/NuamExchange.Api' usando una cadena SQL Server válida para la base 'NuamTributariaDB'.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<NuamExchangeDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new NuamExchangeDbContext(optionsBuilder.Options);
    }

    private static string ResolveApiProjectPath()
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (currentDirectory is not null)
        {
            var candidatePath = Path.Combine(currentDirectory.FullName, ApiProjectRelativePath);

            if (Directory.Exists(candidatePath))
            {
                return candidatePath;
            }

            currentDirectory = currentDirectory.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
