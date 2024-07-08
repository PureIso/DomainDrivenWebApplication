using System.Text.Json.Serialization;
using DomainDrivenWebApplication.Domain.Interfaces;
using DomainDrivenWebApplication.Domain.Services;
using DomainDrivenWebApplication.Infrastructure.Data;
using DomainDrivenWebApplication.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using DomainDrivenWebApplication.API.Models;
using DomainDrivenWebApplication.Domain.Entities;
using OpenTelemetry.Logs;
using DomainDrivenWebApplication.API.Extensions;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Load configuration from appsettings.json and environment-specific files
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
string environment = builder.Environment.EnvironmentName;
builder.Configuration.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

// Configure JSON serialization options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;                      // Keep property names as-is (default camelCase)
        options.JsonSerializerOptions.WriteIndented = true;                             // Format JSON responses with indentation
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());    // Serialize enums as strings
    });

// Register AutoMapper with inline mapping profile
builder.Services.AddAutoMapper(cfg =>
{
    cfg.CreateMap<School, SchoolDto>().ReverseMap();
});


// Register services
builder.Services.AddScoped<ISchoolRepository, SchoolRepository>();
builder.Services.AddScoped<SchoolService>();

// Add DbContext with connection string from appsettings
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SchoolContext>(options =>
    options.UseSqlServer(connectionString));

// Add Swagger for API documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "School API", Version = "v1" });
});

// Configure OpenTelemetry
string? openTelemetryEndpoint = builder.Configuration["OTLP_ENDPOINT_URL"];
OpenTelemetryBuilder openTelemetry = builder.Services.AddOpenTelemetry();

// Configure OpenTelemetry Resources with the application name
openTelemetry.ConfigureResource(resource => resource
    .AddService(serviceName: builder.Environment.ApplicationName));

// Add Metrics for ASP.NET Core
openTelemetry.WithMetrics(metrics =>
{
    // Metrics provider from OpenTelemetry
    metrics.AddAspNetCoreInstrumentation();
    metrics.AddMeter("SchoolAPI");
    // Metrics provides by ASP.NET Core in .NET 8
    metrics.AddMeter("Microsoft.AspNetCore.Hosting");
    metrics.AddMeter("Microsoft.AspNetCore.Server.Kestrel");
    if (openTelemetryEndpoint != null)
    {
        //Prometheus
        //AppInsights
        metrics.AddOtlpExporter(option =>
        {
            //otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint);
        });
    }
    else
    {
        metrics.AddConsoleExporter();
    }
});

// Add Tracing for ASP.NET Core
openTelemetry.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation();
    tracing.AddHttpClientInstrumentation();
    tracing.AddSource("Activity School API");
    if (openTelemetryEndpoint != null)
    {
        //Prometheus
        //AppInsights
        tracing.AddOtlpExporter(option =>
        {
            //otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint);
        });
    }
    else
    {
        tracing.AddConsoleExporter();
    }
});

// Add Logging with OpenTelemetry
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddOpenTelemetry(options =>
{
    options.AddConsoleExporter();
    if (openTelemetryEndpoint != null)
    {
        options.AddOtlpExporter(otlpOptions =>
        {
            // otlpOptions.Endpoint = new Uri(openTelemetryEndpoint);
        });
    }
});

// Add custom redacting logger provider
builder.Logging.AddProvider(new CustomRedactingLoggerProvider());

// Apply filters to avoid logging sensitive data below Information level
builder.Logging.AddFilter((category, level) => level >= LogLevel.Information);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "School API v1"));
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created and apply any pending migrations
using (IServiceScope scope = app.Services.CreateScope())
{
    SchoolContext dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();
    dbContext.Database.Migrate(); // Apply any pending migrations
}

app.Run();
