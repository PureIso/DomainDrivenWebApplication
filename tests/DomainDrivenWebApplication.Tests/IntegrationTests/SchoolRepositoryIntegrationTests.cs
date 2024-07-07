using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Infrastructure.Repositories;
using DomainDrivenWebApplication.Tests.Fixtures;

namespace DomainDrivenWebApplication.Tests.IntegrationTests;

[Collection("SchoolRepositoryTests")]
public class SchoolRepositoryIntegrationTests : IClassFixture<SchoolFixture>, IDisposable, IAsyncLifetime
{
    private readonly SchoolFixture _fixture;

    public SchoolRepositoryIntegrationTests(SchoolFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Can_Add_and_Get_School()
    {
        // Arrange
        SchoolRepository? repository = _fixture.SchoolRepository;
        Assert.NotNull(repository);

        School school = new School { Name = "School 1", Address = "Test Address", PrincipalName = "John Doe" };

        // Act
        await repository.AddAsync(school);
        School? retrievedSchool = await repository.GetByIdAsync(school.Id);

        // Assert
        Assert.NotNull(retrievedSchool);
        Assert.Equal(school.Name, retrievedSchool.Name);
    }

    [Fact]
    public async Task Can_Get_All_Schools()
    {
        // Arrange
        SchoolRepository? repository = _fixture.SchoolRepository;
        Assert.NotNull(repository);

        School school1 = new School { Name = "School 1", Address = "Test Address", PrincipalName = "John Doe" };
        School school2 = new School { Name = "School 2", Address = "Test Address", PrincipalName = "John Doe" };

        await repository.AddAsync(school1);
        await repository.AddAsync(school2);

        // Act
        List<School>? schools = await repository.GetAllAsync();

        // Assert
        Assert.NotNull(schools);
        Assert.Contains(schools, s => s.Name == "School 1");
        Assert.Contains(schools, s => s.Name == "School 2");
    }

    [Fact]
    public async Task Can_Update_School()
    {
        // Arrange
        SchoolRepository? repository = _fixture.SchoolRepository;
        Assert.NotNull(repository);

        School school = new School { Name = "Test School", Address = "Test Address", PrincipalName = "John Doe" };
        await repository.AddAsync(school);

        // Act
        school.Name = "Updated School";
        await repository.UpdateAsync(school);
        School? updatedSchool = await repository.GetByIdAsync(school.Id);

        // Assert
        Assert.NotNull(updatedSchool);
        Assert.Equal("Updated School", updatedSchool.Name);
    }

    [Fact]
    public async Task Can_Delete_School()
    {
        // Arrange
        SchoolRepository? repository = _fixture.SchoolRepository;
        Assert.NotNull(repository);

        School school = new School { Name = "Test School", Address = "Test Address", PrincipalName = "John Doe" };
        await repository.AddAsync(school);

        // Act
        await repository.DeleteAsync(school);
        School? deletedSchool = await repository.GetByIdAsync(school.Id);

        // Assert
        Assert.Null(deletedSchool);
    }

    [Fact]
    public async Task Can_Get_Schools_By_Date_Range()
    {
        // Arrange
        SchoolRepository? repository = _fixture.SchoolRepository;
        Assert.NotNull(repository);

        School school = new School { Name = "School 1", Address = "Test Address", PrincipalName = "John Doe" };
        bool inserted = await repository.AddAsync(school);
        Assert.True(inserted);

        DateTime fromDate = DateTime.UtcNow;
        School? retrievedSchool = await repository.GetByIdAsync(1);
        Assert.NotNull(retrievedSchool);

        retrievedSchool.Name = "School 2";
        await repository.UpdateAsync(retrievedSchool);

        // Act
        List<School>? schools = await repository.GetSchoolsByDateRangeAsync(fromDate.AddDays(5), DateTime.MaxValue);


        // Assert
        Assert.NotNull(schools);
        Assert.Contains(schools, s => s.Name == "School 2");
        Assert.DoesNotContain(schools, s => s.Name == "School 1");
    }

    [Fact]
    public async Task Can_Get_All_Versions_Of_School()
    {
        // Arrange
        SchoolRepository? repository = _fixture.SchoolRepository;
        Assert.NotNull(repository);

        // Create and add the initial version of the school
        School school = new School { Name = "School 1", Address = "Test Address", PrincipalName = "John Doe" };

        bool inserted = await repository.AddAsync(school);
        Assert.True(inserted);

        // Update the school to create a new version
        School? retrievedSchool = await repository.GetByIdAsync(1);
        Assert.NotNull(retrievedSchool);
        retrievedSchool.Name = "School 2";
        retrievedSchool.Address = "Address 2";

        await repository.UpdateAsync(retrievedSchool);


        // Act
        List<School>? schools = await repository.GetAllVersionsAsync(1);

        // Assert
        Assert.NotNull(schools);
        Assert.Equal(2, schools.Count); // Expecting two versions

        // Optional: Assert specific properties of each version if needed
        Assert.Equal("School 1", schools[0].Name);
        Assert.Equal("School 2", schools[1].Name);
    }

    private async Task CleanupData()
    {
        if (_fixture.SchoolRepository != null)
        {
            List<School>? schools = await _fixture.SchoolRepository.GetAllAsync();
            if (schools != null)
            {
                foreach (School school in schools)
                {
                    await _fixture.SchoolRepository.DeleteAsync(school);
                }
            }
        }
    }

    // Ensure cleanup after each test
    public async Task InitializeAsync()
    {
        await _fixture.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }
}