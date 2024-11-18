using DomainDrivenWebApplication.Domain.Services;
using DomainDrivenWebApplication.Infrastructure.Data;
using DomainDrivenWebApplication.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace DomainDrivenWebApplication.Tests.Fixtures
{
    public class SchoolFixture : IAsyncLifetime
    {
        private MsSqlContainer _msSqlContainer;
        public SchoolRepository? SchoolRepository { get; private set; } = null;
        public SchoolService? SchoolService { get; private set; } = null;
        public SchoolContext? SchoolContext { get; private set; } = null;

        public SchoolFixture()
        {
            _msSqlContainer = new MsSqlBuilder().Build();
        }

        public async Task InitializeAsync()
        {
            _msSqlContainer = new MsSqlBuilder().Build();
            await _msSqlContainer.StartAsync();

            DbContextOptions<SchoolContext> options = new DbContextOptionsBuilder<SchoolContext>()
                .UseSqlServer(_msSqlContainer.GetConnectionString())
                .Options;

            SchoolContext = new SchoolContext(options);
            await SchoolContext.Database.EnsureCreatedAsync();

            // Set up the ServiceCollection for Dependency Injection
            ServiceCollection serviceCollection = new ServiceCollection();

            // Register ILoggerFactory (needed by ResourceManagerStringLocalizerFactory)
            serviceCollection.AddLogging();

            // Configure localization services
            serviceCollection.AddLocalization(options => options.ResourcesPath = "Resources");
            serviceCollection.AddDbContext<SchoolContext>(opts => opts.UseSqlServer(_msSqlContainer.GetConnectionString()));

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            // Resolve IStringLocalizer and repository
            IStringLocalizer<SchoolRepository> localizer = serviceProvider.GetRequiredService<IStringLocalizer<SchoolRepository>>();

            SchoolRepository = new SchoolRepository(SchoolContext, localizer);
            SchoolService = new SchoolService(SchoolRepository);
        }

        public async Task DisposeAsync()
        {
            await _msSqlContainer.DisposeAsync();
        }

        public void Dispose()
        {
            SchoolContext?.Dispose();
            DisposeAsync().GetAwaiter().GetResult();
        }
    }
}
