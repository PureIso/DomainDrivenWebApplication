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

namespace DomainDrivenWebApplication.Infrastructure;

/// <summary>
/// Provides extension methods for configuring and initializing infrastructure services, 
/// including database contexts, repositories, and telemetry instrumentation.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Configures and registers infrastructure services such as database contexts, repositories, and OpenTelemetry instrumentation.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the services are added.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> instance to retrieve configuration values.</param>
    /// <returns>The configured <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        string serviceType = Environment.GetEnvironmentVariable("SERVICE_TYPE") ?? "default";

        if (serviceType.Equals("reader", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<QuerySchoolContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<ISchoolQueryRepository, SchoolQueryRepository>();
        }
        else if (serviceType.Equals("writer", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<CommandSchoolContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<ISchoolCommandRepository, SchoolCommandRepository>();
        }
        else
        {
            services.AddDbContext<SchoolContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<ISchoolRepository, SchoolRepository>();
        }

        services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService($"domaindrivenwebapplication.api-{serviceType}"));
                metrics.AddRuntimeInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddConsoleExporter();
            })
            .WithTracing(tracing =>
            {
                tracing.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService($"domaindrivenwebapplication.api-{serviceType}"));
                tracing.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter();
            })
            .WithLogging(logging =>
            {
                logging.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService($"domaindrivenwebapplication.api-{serviceType}"));
            });

        return services;
    }

    /// <summary>
    /// Ensures that the database is up-to-date by applying pending migrations.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance used to access application services.</param>
    public static void EnsureDatabase(WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IServiceProvider serviceProvider = scope.ServiceProvider;

        string serviceType = Environment.GetEnvironmentVariable("SERVICE_TYPE") ?? "default";

        if (serviceType.Equals("reader", StringComparison.OrdinalIgnoreCase))
        {
            QuerySchoolContext queryContext = serviceProvider.GetRequiredService<QuerySchoolContext>();
            ApplyMigrations(queryContext);
        }
        else if (serviceType.Equals("writer", StringComparison.OrdinalIgnoreCase))
        {
            CommandSchoolContext commandContext = serviceProvider.GetRequiredService<CommandSchoolContext>();
            ApplyMigrations(commandContext);
        }
        else
        {
            SchoolContext schoolContext = serviceProvider.GetRequiredService<SchoolContext>();
            ApplyMigrations(schoolContext);
        }
    }

    /// <summary>
    /// Applies pending migrations for the specified <see cref="DbContext"/>.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/> for which migrations are applied.</param>
    private static void ApplyMigrations(DbContext context)
    {
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
    }
}
