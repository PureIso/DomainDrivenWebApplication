using Asp.Versioning;
using DomainDrivenWebApplication.API.Middleware;
using DomainDrivenWebApplication.API.Models;
using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Domain.Interfaces;
using DomainDrivenWebApplication.Domain.Services;
using DomainDrivenWebApplication.Infrastructure.Data;
using DomainDrivenWebApplication.Infrastructure.Logging;
using DomainDrivenWebApplication.Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Reflection;
using System.Text.Json.Serialization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure Serilog
bool useRelaxedEscaping = builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "Docker";
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
    .WriteTo.Console(new CustomSerilogJsonFormatter(useRelaxedEscaping))
    .CreateLogger();

builder.Host.UseSerilog();

// Check if running in Docker
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Docker")
{
    builder.Configuration.AddUserSecrets<Program>();
    builder.Configuration.AddEnvironmentVariables();
}

// Configure JSON serialization options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddApiVersioning(options =>
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
Log.Information("Connection: {connectionString}", connectionString);
builder.Services.AddDbContext<SchoolContext>(options =>
    options.UseSqlServer(connectionString));

// Add Swagger for API documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "School API", Version = "v1" });
    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
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

WebApplication app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "Docker")
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "School API v1"));
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created and apply any pending migrations
using (IServiceScope scope = app.Services.CreateScope())
{
    SchoolContext dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();
    dbContext.Database.Migrate();
}

// Logging Kestrel and HTTPS certificate information
ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();
IServer server = app.Services.GetRequiredService<IServer>();
ICollection<string>? addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;

if (addresses != null && addresses.Any())
{
    foreach (string address in addresses)
    {
        logger.LogInformation("Kestrel is listening on: {Address}", address);
    }
}
else
{
    logger.LogInformation("Kestrel addresses are empty.");
}

// Log application startup
logger.LogInformation("Application has started successfully.");

app.Run();
