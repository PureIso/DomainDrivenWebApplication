﻿namespace DomainDrivenWebApplication.Domain;

using DomainDrivenWebApplication.Domain.Common.Models;
using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

public static class DomainServiceCollectionExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        // Register domain services
        services.AddScoped<SchoolService>();
        // Register AutoMapper with inline mapping profile
        services.AddAutoMapper(cfg =>
        {
            cfg.CreateMap<School, SchoolDto>().ReverseMap();
        });

        // Configure JSON serialization options
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        return services;
    }
}
