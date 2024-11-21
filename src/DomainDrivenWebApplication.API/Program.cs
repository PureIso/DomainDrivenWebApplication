using DomainDrivenWebApplication.API.Middleware;
using DomainDrivenWebApplication.Infrastructure.Logging;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Serilog;
using DomainDrivenWebApplication.Infrastructure;
using DomainDrivenWebApplication.Domain;
using DomainDrivenWebApplication.API;

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

builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddDomain()
    .AddAPI();


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

app.UseLocalizationMiddleware();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

InfrastructureServiceCollectionExtensions.EnsureDatabase(app);

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
