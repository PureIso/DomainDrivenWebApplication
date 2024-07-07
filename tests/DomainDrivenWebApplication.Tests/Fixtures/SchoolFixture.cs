using DomainDrivenWebApplication.Domain.Services;
using DomainDrivenWebApplication.Infrastructure.Data;
using DomainDrivenWebApplication.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Testcontainers.MsSql;

namespace DomainDrivenWebApplication.Tests.Fixtures;

public class SchoolFixture : IAsyncLifetime
{
    private MsSqlContainer _msSqlContainer;
    private ILogger<SchoolFixture> _logger;

    public SchoolRepository? SchoolRepository { get; private set; } = null;
    public SchoolService? SchoolService { get; private set; } = null;
    public SchoolContext? SchoolContext { get; private set; } = null;

    public async Task InitializeAsync()
    {
        try
        {
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<SchoolFixture>();
            _logger.LogInformation("Starting SQL container...");
            _msSqlContainer = new MsSqlBuilder().Build();
            
            await _msSqlContainer.StartAsync();

            DbContextOptions<SchoolContext> options = new DbContextOptionsBuilder<SchoolContext>()
                .UseSqlServer(_msSqlContainer.GetConnectionString())
                .Options;

            _logger.LogInformation("Creating database and enabling temporal table...");
            // Create SchoolContext and ensure the database is created
            SchoolContext = new SchoolContext(options);
            await SchoolContext.Database.EnsureCreatedAsync();


            SchoolRepository = new SchoolRepository(SchoolContext);
            SchoolService = new SchoolService(SchoolRepository);
            _logger.LogInformation("Initialization complete.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Initialization failed");
            throw;
        }
    }

    private async Task ExecuteSqlScript(string scriptFileName)
    {
        string script = await File.ReadAllTextAsync(scriptFileName);
        await SchoolContext.Database.ExecuteSqlRawAsync(script);
    }

    public async Task DisposeAsync()
    {
        try
        {
            _logger.LogInformation("Disposing SQL container...");
            await _msSqlContainer.DisposeAsync();
            _logger.LogInformation("Disposal complete.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Disposal failed");
            throw;
        }
    }

    public void Dispose()
    {
        SchoolContext?.Dispose();
        DisposeAsync().GetAwaiter().GetResult();
    }
}