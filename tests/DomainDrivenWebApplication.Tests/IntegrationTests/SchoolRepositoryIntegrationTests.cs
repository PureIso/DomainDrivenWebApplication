using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Infrastructure.Repositories;
using DomainDrivenWebApplication.Tests.Fixtures;
using ErrorOr;

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
        ErrorOr<bool> addResult = await repository.AddAsync(school);
        Assert.False(addResult.IsError, "Failed to add school.");

        ErrorOr<School> getResult = await repository.GetByIdAsync(school.Id);

        // Assert
        Assert.False(getResult.IsError, "Failed to retrieve school.");
        School retrievedSchool = getResult.Value;
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
        ErrorOr<List<School>> getAllResult = await repository.GetAllAsync();

        // Assert
        Assert.False(getAllResult.IsError, "Failed to retrieve schools.");
        List<School> schools = getAllResult.Value;
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
        ErrorOr<bool> addResult = await repository.AddAsync(school);
        Assert.False(addResult.IsError, "Failed to add school.");

        // Act
        school.Name = "Updated School";
        ErrorOr<bool> updateResult = await repository.UpdateAsync(school);
        Assert.False(updateResult.IsError, "Failed to update school.");

        ErrorOr<School> updatedResult = await repository.GetByIdAsync(school.Id);

        // Assert
        Assert.False(updatedResult.IsError, "Failed to retrieve updated school.");
        School updatedSchool = updatedResult.Value;
        Assert.Equal("Updated School", updatedSchool.Name);
    }

    [Fact]
    public async Task Can_Delete_School()
    {
        // Arrange
        SchoolRepository? repository = _fixture.SchoolRepository;
        Assert.NotNull(repository);

        School school = new School { Name = "Test School", Address = "Test Address", PrincipalName = "John Doe" };
        ErrorOr<bool> addResult = await repository.AddAsync(school);
        Assert.False(addResult.IsError, "Failed to add school.");

        // Act
        ErrorOr<bool> deleteResult = await repository.DeleteAsync(school);
        Assert.False(deleteResult.IsError, "Failed to delete school.");

        ErrorOr<School> deletedSchoolResult = await repository.GetByIdAsync(school.Id);

        // Assert
        Assert.True(deletedSchoolResult.IsError, "School should have been deleted.");
        Assert.Null(deletedSchoolResult.Value);
    }

    [Fact]
    public async Task Can_Get_Schools_By_Date_Range()
    {
        // Arrange
        SchoolRepository? repository = _fixture.SchoolRepository;
        Assert.NotNull(repository);

        School school = new School { Name = "School 1", Address = "Test Address", PrincipalName = "John Doe" };
        ErrorOr<bool> insertedResult = await repository.AddAsync(school);
        Assert.False(insertedResult.IsError, "Failed to insert school.");

        DateTime fromDate = DateTime.UtcNow;
        ErrorOr<School> retrievedSchoolResult = await repository.GetByIdAsync(1);
        Assert.False(retrievedSchoolResult.IsError, "Failed to retrieve school.");

        School retrievedSchool = retrievedSchoolResult.Value;
        retrievedSchool.Name = "School 2";
        ErrorOr<bool> updateResult = await repository.UpdateAsync(retrievedSchool);
        Assert.False(updateResult.IsError, "Failed to update school.");

        // Act
        ErrorOr<List<School>> schoolsResult = await repository.GetSchoolsByDateRangeAsync(fromDate.AddDays(5), DateTime.MaxValue);

        // Assert
        Assert.False(schoolsResult.IsError, "Failed to retrieve schools by date range.");
        List<School> schools = schoolsResult.Value;
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
        ErrorOr<bool> insertedResult = await repository.AddAsync(school);
        Assert.False(insertedResult.IsError, "Failed to add school.");

        // Update the school to create a new version
        ErrorOr<School> retrievedSchoolResult = await repository.GetByIdAsync(1);
        Assert.False(retrievedSchoolResult.IsError, "Failed to retrieve school.");
        School retrievedSchool = retrievedSchoolResult.Value;

        retrievedSchool.Name = "School 2";
        retrievedSchool.Address = "Address 2";
        ErrorOr<bool> updateResult = await repository.UpdateAsync(retrievedSchool);
        Assert.False(updateResult.IsError, "Failed to update school.");

        // Act
        ErrorOr<List<School>> allVersionsResult = await repository.GetAllVersionsAsync(1);

        // Assert
        Assert.False(allVersionsResult.IsError, "Failed to retrieve all versions.");
        List<School> allVersions = allVersionsResult.Value;
        Assert.Equal(2, allVersions.Count);

        Assert.Equal("School 1", allVersions[0].Name);
        Assert.Equal("School 2", allVersions[1].Name);
    }

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
