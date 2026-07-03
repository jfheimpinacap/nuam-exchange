using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NuamExchange.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
ValidateProductionConfiguration(builder.Configuration, builder.Environment);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();

        if (exceptionFeature?.Error is not null)
        {
            logger.LogError(exceptionFeature.Error, "Unhandled exception while processing the request.");
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Se produjo un error inesperado.",
            Detail = "La solicitud no pudo ser procesada de forma segura. Intente nuevamente más tarde."
        };

        await context.Response.WriteAsJsonAsync(problem);
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

static void ValidateProductionConfiguration(IConfiguration configuration, IWebHostEnvironment environment)
{
    if (environment.IsDevelopment())
    {
        return;
    }

    var missingOrInvalid = new List<string>();
    if (string.IsNullOrWhiteSpace(configuration.GetConnectionString("NuamTributariaDb")))
    {
        missingOrInvalid.Add("ConnectionStrings:NuamTributariaDb");
    }

    var signingKey = configuration["Jwt:SigningKey"];
    if (string.IsNullOrWhiteSpace(signingKey) || System.Text.Encoding.UTF8.GetByteCount(signingKey) < 32)
    {
        missingOrInvalid.Add("Jwt:SigningKey");
    }

    if (missingOrInvalid.Count > 0)
    {
        throw new InvalidOperationException($"Production configuration is missing or invalid: {string.Join(", ", missingOrInvalid)}.");
    }
}

public partial class Program
{
}
