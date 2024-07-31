using System.Text.Json.Serialization;
using DomainDrivenWebApplication.Domain.Interfaces;
using DomainDrivenWebApplication.Domain.Services;
using DomainDrivenWebApplication.Infrastructure.Data;
using DomainDrivenWebApplication.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using DomainDrivenWebApplication.API.Models;
using DomainDrivenWebApplication.Domain.Entities;
using OpenTelemetry.Logs;
using Serilog;
using System.Reflection;
using Asp.Versioning;
using DomainDrivenWebApplication.Infrastructure.Logging;
using DomainDrivenWebApplication.API.Middleware;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true);
builder.Configuration.AddEnvironmentVariables();

bool useRelaxedEscaping = builder.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Docker";
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
    .WriteTo.Console(new CustomSerilogJsonFormatter(useRelaxedEscaping))
    .CreateLogger();
builder.Host.UseSerilog();

if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Docker")
{
    builder.Configuration.AddUserSecrets<Program>();
    builder.Configuration.AddEnvironmentVariables(); // overwrites previous values
}

// Configure JSON serialization options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;                      // Keep property names as-is (default camelCase)
        options.JsonSerializerOptions.WriteIndented = true;                             // Format JSON responses with indentation
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());    // Serialize enums as strings
    });

builder.Services.AddApiVersioning(options =>
{
    // Specify the default API version
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
builder.Services.AddDbContext<SchoolContext>(options =>
    options.UseSqlServer(connectionString));

// Add Swagger for API documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "School API", Version = "v1" });
    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if(File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("conveyor.controller"));
        metrics.AddRuntimeInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddConsoleExporter();
    })
    .WithTracing(tracing =>
    {
        tracing.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("conveyor.controller"));
        tracing.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    })
    .WithLogging(logging =>
    {
        logging.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("conveyor.controller"));
    });

WebApplication app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "School API v1"));
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseMiddleware<CorrelationIdMiddleware>();
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
