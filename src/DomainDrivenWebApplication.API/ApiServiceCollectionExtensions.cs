namespace DomainDrivenWebApplication.API;

using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Asp.Versioning;
using System.Text.Json.Serialization;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddAPI(this IServiceCollection services)
    {
        // Add versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // Add Swagger
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "School API", Version = "v1" });

            // Include XML comments if available
            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });

        // Add Controllers
        services.AddControllers()
        .AddDataAnnotationsLocalization()
        .AddViewLocalization()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
            options.JsonSerializerOptions.WriteIndented = true;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }
}
