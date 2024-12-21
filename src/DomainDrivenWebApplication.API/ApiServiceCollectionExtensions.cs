namespace DomainDrivenWebApplication.API;

using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Asp.Versioning;
using System.Text.Json.Serialization;

/// <summary>
/// Provides extension methods to configure API services for the application.
/// </summary>
public static class ApiServiceCollectionExtensions
{
    /// <summary>
    /// Configures API-related services, including versioning, Swagger, and controller support.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
    /// <remarks>
    /// This method performs the following configurations:
    /// <list type="bullet">
    /// <item><description>API Versioning: Default version is set to 1.0. Versioning is based on URL segments.</description></item>
    /// <item><description>Swagger: Dynamically configures Swagger based on the <c>ServiceType</c> in the app settings.</description></item>
    /// <item><description>Controllers: Configures controllers with localization and JSON serialization options.</description></item>
    /// </list>
    /// The <c>ServiceType</c> (default, reader, writer) influences Swagger grouping and filtering.
    /// </remarks>
    public static IServiceCollection AddApi(this IServiceCollection services)
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

        // Resolve the ServiceType from configuration
        IServiceProvider serviceProvider = services.BuildServiceProvider();
        IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
        string serviceType = configuration.GetValue<string>("ServiceType") ?? "default";

        // Add Swagger
        services.AddSwaggerGen(swaggerGenOption =>
        {
            swaggerGenOption.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = $"School API - {serviceType.ToUpperInvariant()}",
                Version = "v1",
                Description = $"API documentation for the {serviceType} service type."
            });

            // Include XML comments if available
            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                swaggerGenOption.IncludeXmlComments(xmlPath);
            }

            // Filter operations by ServiceType using GroupName
            swaggerGenOption.DocInclusionPredicate((docName, apiDesc) => string.IsNullOrEmpty(apiDesc.GroupName) ||
                                                                         apiDesc.GroupName.Equals(serviceType, StringComparison.OrdinalIgnoreCase) || 
                                                                         (serviceType == "default" && 
                                                                          (apiDesc.GroupName.Equals("reader", StringComparison.OrdinalIgnoreCase) || 
                                                                           apiDesc.GroupName.Equals("writer", StringComparison.OrdinalIgnoreCase))));

            swaggerGenOption.TagActionsBy(api => [api.GroupName ?? "default"]);
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
