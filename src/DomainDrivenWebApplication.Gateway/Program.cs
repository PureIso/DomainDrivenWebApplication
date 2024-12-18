using Ocelot.DependencyInjection;
using Ocelot.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Get the current environment (e.g., Development, Production, Docker)
string environment = builder.Environment.EnvironmentName;

// Add the corresponding ocelot configuration file
string ocelotConfigFile = $"ocelot.{environment}.json";
builder.Configuration.AddJsonFile(ocelotConfigFile, optional: true, reloadOnChange: true);
builder.Services.AddOcelot();

WebApplication app = builder.Build();
await app.UseOcelot();

app.Run();
