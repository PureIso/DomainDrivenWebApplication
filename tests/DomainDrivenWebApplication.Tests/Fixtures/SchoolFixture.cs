using DomainDrivenWebApplication.Domain.Services;
using DomainDrivenWebApplication.Infrastructure.Data;
using DomainDrivenWebApplication.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace DomainDrivenWebApplication.Tests.Fixtures;

/// <summary>
/// A test fixture that initializes and disposes resources necessary for testing
/// the School-related services and repositories. It manages the lifecycle of
/// a SQL Server container, sets up database contexts, and configures dependency injection.
/// </summary>
public class SchoolFixture : IAsyncLifetime
{
    private MsSqlContainer _msSqlContainer;
    public SchoolRepository SchoolRepository { get; private set; }
    public SchoolCommandRepository SchoolCommandRepository { get; private set; }
    public SchoolQueryRepository SchoolQueryRepository { get; private set; }
    public SchoolServiceCommandQuery SchoolServiceCommandQuery { get; private set; }
    public SchoolService SchoolService { get; private set; }
    public SchoolContext SchoolContext { get; private set; }
    public SchoolCommandContext SchoolCommandContext { get; private set; }
    public SchoolQueryContext SchoolQueryContext { get; private set; }

    /// <summary>
    /// Initializes the test fixture by setting up the SQL Server container
    /// and configuring necessary database contexts and repositories.
    /// </summary>
    public SchoolFixture()
    {
        _msSqlContainer = new MsSqlBuilder().Build();
    }

    /// <summary>
    /// Asynchronously initializes the resources required for the tests,
    /// including starting the SQL Server container, creating database contexts,
    /// and setting up dependency injection for repositories and services.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InitializeAsync()
    {
        _msSqlContainer = new MsSqlBuilder().Build();
        await _msSqlContainer.StartAsync();

        DbContextOptions<SchoolContext> optionsSchoolContext = new DbContextOptionsBuilder<SchoolContext>()
            .UseSqlServer(_msSqlContainer.GetConnectionString())
            .Options;
        DbContextOptions<SchoolCommandContext> optionsSchoolCommandContext = new DbContextOptionsBuilder<SchoolCommandContext>()
            .UseSqlServer(_msSqlContainer.GetConnectionString())
            .Options;
        DbContextOptions<SchoolQueryContext> optionsSchoolQueryContext = new DbContextOptionsBuilder<SchoolQueryContext>()
            .UseSqlServer(_msSqlContainer.GetConnectionString())
            .Options;

        SchoolContext = new SchoolContext(optionsSchoolContext);
        await SchoolContext.Database.EnsureCreatedAsync();
        SchoolCommandContext = new SchoolCommandContext(optionsSchoolCommandContext);
        await SchoolCommandContext.Database.EnsureCreatedAsync();
        SchoolQueryContext = new SchoolQueryContext(optionsSchoolQueryContext);
        await SchoolQueryContext.Database.EnsureCreatedAsync();

        ServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();

        serviceCollection.AddLocalization(options => options.ResourcesPath = "Resources");
        serviceCollection.AddDbContextPool<SchoolContext>(opts => opts.UseSqlServer(_msSqlContainer.GetConnectionString()));
        serviceCollection.AddDbContextFactory<SchoolCommandContext>(opts => opts.UseSqlServer(_msSqlContainer.GetConnectionString()));
        serviceCollection.AddDbContextFactory<SchoolQueryContext>(opts => opts.UseSqlServer(_msSqlContainer.GetConnectionString()));
        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        IStringLocalizer<SchoolRepository> localizer = serviceProvider.GetRequiredService<IStringLocalizer<SchoolRepository>>();
        IStringLocalizer<SchoolCommandRepository> localizerSchoolCommandRepository = serviceProvider.GetRequiredService<IStringLocalizer<SchoolCommandRepository>>();
        IStringLocalizer<SchoolQueryRepository> localizerSchoolQueryRepository = serviceProvider.GetRequiredService<IStringLocalizer<SchoolQueryRepository>>();

        IDbContextFactory<SchoolCommandContext> schoolCommandContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<SchoolCommandContext>>();
        IDbContextFactory<SchoolQueryContext> schoolQueryContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<SchoolQueryContext>>();

        SchoolRepository = new SchoolRepository(SchoolContext, localizer);
        SchoolCommandRepository = new SchoolCommandRepository(schoolCommandContextFactory, localizerSchoolCommandRepository);
        SchoolQueryRepository = new SchoolQueryRepository(schoolQueryContextFactory, localizerSchoolQueryRepository);

        SchoolServiceCommandQuery = new SchoolServiceCommandQuery(SchoolCommandRepository, SchoolQueryRepository);
        SchoolService = new SchoolService(SchoolRepository);
    }

    /// <summary>
    /// Asynchronously disposes of the resources, including the SQL Server container.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
    }

    /// <summary>
    /// Disposes of the resources synchronously, including disposing of the
    /// SchoolContext and the SQL Server container.
    /// </summary>
    public async Task Dispose()
    {
        await SchoolContext.DisposeAsync();
        await DisposeAsync();
    }
}