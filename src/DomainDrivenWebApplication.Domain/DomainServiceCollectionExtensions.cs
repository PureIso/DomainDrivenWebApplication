namespace DomainDrivenWebApplication.Domain;

using Common.Models;
using Entities;
using Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring domain services in the dependency injection container.
/// </summary>
public static class DomainServiceCollectionExtensions
{
    /// <summary>
    /// Adds domain-specific services and configurations to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddScoped<SchoolServiceCommandQuery>();
        services.AddScoped<SchoolService>();
        services.AddAutoMapper(cfg =>
        {
            cfg.CreateMap<School, SchoolDto>().ReverseMap();
        });
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services;
    }
}