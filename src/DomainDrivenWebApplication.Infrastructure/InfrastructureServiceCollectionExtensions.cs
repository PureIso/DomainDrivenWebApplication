namespace DomainDrivenWebApplication.Infrastructure;

using DomainDrivenWebApplication.Domain.Interfaces;
using DomainDrivenWebApplication.Infrastructure.Data;
using DomainDrivenWebApplication.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext with connection string from appsettings
        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<SchoolContext>(options =>
            options.UseSqlServer(connectionString));

        // Configure OpenTelemetry
        services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("domaindrivenwebapplication.api"));
                metrics.AddRuntimeInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddConsoleExporter();
            })
            .WithTracing(tracing =>
            {
                tracing.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("domaindrivenwebapplication.api"));
                tracing.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter();
            })
            .WithLogging(logging =>
            {
                logging.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("domaindrivenwebapplication.api"));
            });

        // Register repository implementations
        services.AddScoped<ISchoolRepository, SchoolRepository>();

        return services;
    }

    public static void EnsureDatabase(WebApplication app)
    {
        // Ensure database is created and apply any pending migrations
        using (IServiceScope scope = app.Services.CreateScope())
        {
            SchoolContext dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();
            dbContext.Database.Migrate();
        }
    }
}
