using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NuamExchange.Application.Administration;
using NuamExchange.Application.Security;
using NuamExchange.Application.TaxClassifications;
using NuamExchange.Infrastructure.Administration;
using NuamExchange.Infrastructure.Authentication;
using NuamExchange.Infrastructure.Persistence;
using NuamExchange.Infrastructure.Seeding;
using NuamExchange.Infrastructure.TaxClassifications;

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

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton<JwtConfigurationState>();
        services.AddSingleton<IPasswordPolicy, DefaultPasswordPolicy>();
        services.AddSingleton<IRoleManagementPolicy, DefaultRoleManagementPolicy>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IAdministrationService, AdministrationService>();
        services.AddSingleton<ITaxClassificationQueryValidator, TaxClassificationQueryValidator>();
        services.AddSingleton<ICreateTaxClassificationValidator, CreateTaxClassificationValidator>();
        services.AddSingleton<IUpdateTaxClassificationValidator, UpdateTaxClassificationValidator>();
        services.AddScoped<ITaxClassificationQueryService, TaxClassificationQueryService>();
        services.AddScoped<ITaxClassificationCommandService, TaxClassificationCommandService>();
        services.AddScoped<IAccessTokenService, JwtAccessTokenService>();
        services.AddScoped<ISecuritySeedService, SecuritySeedService>();

        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
        var signingKey = string.IsNullOrWhiteSpace(jwtSettings.SigningKey)
            ? new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32))
            : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = false;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdministratorOnly", policy => policy.RequireRole(SecuritySeedService.AdministratorRole));
            options.AddPolicy("TaxAnalystOrAdministrator", policy => policy.RequireRole(SecuritySeedService.TaxAnalystRole, SecuritySeedService.AdministratorRole));
            options.AddPolicy("SupervisorOrAdministrator", policy => policy.RequireRole(SecuritySeedService.SupervisorRole, SecuritySeedService.AdministratorRole));
            options.AddPolicy("TaxClassificationRead", policy => policy.RequireRole(SecuritySeedService.AdministratorRole, SecuritySeedService.TaxAnalystRole, SecuritySeedService.SupervisorRole));
            options.AddPolicy("TaxClassificationWrite", policy => policy.RequireRole(SecuritySeedService.AdministratorRole, SecuritySeedService.TaxAnalystRole));
        });

        return services;
    }
}
