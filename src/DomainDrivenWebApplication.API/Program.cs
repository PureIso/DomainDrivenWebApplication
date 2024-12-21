using DomainDrivenWebApplication.Infrastructure.Logging;
using Serilog;
using DomainDrivenWebApplication.Infrastructure;
using DomainDrivenWebApplication.Domain;
using DomainDrivenWebApplication.API;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
string serviceType = Environment.GetEnvironmentVariable("SERVICE_TYPE") ?? "default";

bool useRelaxedEscaping = builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "Docker";
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
    .WriteTo.Console(new CustomSerilogJsonFormatter(useRelaxedEscaping))
    .CreateLogger();
builder.Host.UseSerilog();

if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Docker")
{
    builder.Configuration.AddUserSecrets<Program>();
    builder.Configuration.AddEnvironmentVariables();

    switch (serviceType)
    {
        case "reader":
            builder.Configuration.AddJsonFile("appsettings.Docker.reader.json", optional: true, reloadOnChange: true);
            break;
        case "writer":
            builder.Configuration.AddJsonFile("appsettings.Docker.writer.json", optional: true, reloadOnChange: true);
            break;
        default:
            builder.Configuration.AddJsonFile("appsettings.Docker.json", optional: true, reloadOnChange: true);
            break;
    }
}

builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddDomain()
    .AddApi();

WebApplication app = builder.Build();

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

app.UseApplicationExtensions();
app.MapControllers();

InfrastructureServiceCollectionExtensions.EnsureDatabase(app);

app.Run();
