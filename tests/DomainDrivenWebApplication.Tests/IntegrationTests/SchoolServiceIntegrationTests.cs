using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Domain.Services;
using DomainDrivenWebApplication.Tests.Fixtures;
using ErrorOr;

namespace DomainDrivenWebApplication.Tests.IntegrationTests;

[Collection("SchoolServiceTests")]
public class SchoolServiceIntegrationTests : IClassFixture<SchoolFixture>, IDisposable, IAsyncLifetime
{
    private readonly SchoolFixture _fixture;

    public SchoolServiceIntegrationTests(SchoolFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Can_Add_and_Get_School()
    {
        // Arrange
        SchoolService? service = _fixture.SchoolService;
        Assert.NotNull(service);

        School school = new School { Name = "Test School", Address = "Test Address", PrincipalName = "John Doe" };

        // Act
        ErrorOr<bool> result = await service.AddSchoolAsync(school);

        // Assert
        Assert.False(result.IsError);
        Assert.True(result.Value);

        ErrorOr<School> retrievedResult = await service.GetSchoolByIdAsync(school.Id);
        Assert.False(retrievedResult.IsError);
        School? retrievedSchool = retrievedResult.Value;
        Assert.NotNull(retrievedSchool);
        Assert.Equal(school.Name, retrievedSchool.Name);
    }

    [Fact]
    public async Task Can_Get_All_Schools()
    {
        // Arrange
        SchoolService? service = _fixture.SchoolService;
        Assert.NotNull(service);

        School school1 = new School { Name = "School 1", Address = "Test Address", PrincipalName = "John Doe" };
        School school2 = new School { Name = "School 2", Address = "Test 2 Address", PrincipalName = "John Doe" };

        ErrorOr<bool> result1 = await service.AddSchoolAsync(school1);
        Assert.False(result1.IsError);
        Assert.True(result1.Value);

        ErrorOr<bool> result2 = await service.AddSchoolAsync(school2);
        Assert.False(result2.IsError);
        Assert.True(result2.Value);

        // Act
        ErrorOr<List<School>> schoolsResult = await service.GetAllSchoolsAsync();

        // Assert
        Assert.False(schoolsResult.IsError);
        List<School>? schools = schoolsResult.Value;
        Assert.NotNull(schools);
        Assert.Contains(schools, s => s.Name == "School 1");
        Assert.Contains(schools, s => s.Name == "School 2");
    }

    [Fact]
    public async Task Can_Update_School()
    {
        // Arrange
        SchoolService? service = _fixture.SchoolService;
        Assert.NotNull(service);

        School school = new School { Name = "Test School", Address = "Test Address", PrincipalName = "John Doe" };
        ErrorOr<bool> result = await service.AddSchoolAsync(school);
        Assert.False(result.IsError);
        Assert.True(result.Value);

        // Act
        school.Name = "Updated School";
        ErrorOr<bool> updateResult = await service.UpdateSchoolAsync(school);

        // Assert
        Assert.False(updateResult.IsError); // Ensure no errors during update

        ErrorOr<School> updatedSchoolResult = await service.GetSchoolByIdAsync(school.Id);
        Assert.False(updatedSchoolResult.IsError);
        School? updatedSchool = updatedSchoolResult.Value;
        Assert.NotNull(updatedSchool);
        Assert.Equal("Updated School", updatedSchool.Name);
    }

    [Fact]
    public async Task Can_Delete_School()
    {
        // Arrange
        SchoolService? service = _fixture.SchoolService;
        Assert.NotNull(service);

        School school = new School { Name = "Test School", Address = "Test Address", PrincipalName = "John Doe" };
        ErrorOr<bool> result = await service.AddSchoolAsync(school);
        Assert.False(result.IsError);
        Assert.True(result.Value);

        // Act
        ErrorOr<bool> deleteResult = await service.DeleteSchoolAsync(school.Id);

        // Assert
        Assert.False(deleteResult.IsError);

        ErrorOr<School> deletedSchoolResult = await service.GetSchoolByIdAsync(school.Id);
        Assert.True(deletedSchoolResult.IsError);
    }

    [Fact]
    public async Task Can_Get_Schools_By_Date_Range()
    {
        // Arrange
        SchoolService? service = _fixture.SchoolService;
        Assert.NotNull(service);

        School school = new School { Name = "School 1", Address = "Test Address", PrincipalName = "John Doe", CreatedAt = DateTime.UtcNow };
        ErrorOr<bool> insertResult = await service.AddSchoolAsync(school);
        Assert.False(insertResult.IsError);
        Assert.True(insertResult.Value);

        DateTime fromDate = DateTime.UtcNow;
        ErrorOr<School> retrievedSchoolResult = await service.GetSchoolByIdAsync(school.Id);
        Assert.False(retrievedSchoolResult.IsError);
        School? retrievedSchool = retrievedSchoolResult.Value;
        Assert.NotNull(retrievedSchool);

        // Update the school name
        retrievedSchool.Name = "School 2";
        ErrorOr<bool> updateResult = await service.UpdateSchoolAsync(retrievedSchool);
        Assert.False(updateResult.IsError);

        // Act
        ErrorOr<List<School>> schoolsResult = await service.GetSchoolsByDateRangeAsync(fromDate.AddDays(1), DateTime.MaxValue);

        // Assert
        Assert.False(schoolsResult.IsError);
        List<School>? schools = schoolsResult.Value;
        Assert.NotNull(schools);
        Assert.Contains(schools, s => s.Name == "School 2");
        Assert.DoesNotContain(schools, s => s.Name == "School 1");
    }

    [Fact]
    public async Task Can_Get_All_Versions_Of_School()
    {
        // Arrange
        SchoolService? service = _fixture.SchoolService;
        Assert.NotNull(service);

        // Create and add the initial version of the school
        School school = new School { Name = "School 1", Address = "Test Address", PrincipalName = "John Doe" };
        ErrorOr<bool> insertResult = await service.AddSchoolAsync(school);
        Assert.False(insertResult.IsError);
        Assert.True(insertResult.Value);

        // Update the school to create a new version
        ErrorOr<School> retrievedSchoolResult = await service.GetSchoolByIdAsync(school.Id);
        Assert.False(retrievedSchoolResult.IsError);
        School? retrievedSchool = retrievedSchoolResult.Value;
        Assert.NotNull(retrievedSchool);
        retrievedSchool.Name = "School 2";
        retrievedSchool.Address = "Address 2";

        ErrorOr<bool> updateResult = await service.UpdateSchoolAsync(retrievedSchool);
        Assert.False(updateResult.IsError);

        // Act
        ErrorOr<List<School>> versionsResult = await service.GetAllVersionsOfSchoolAsync(school.Id);

        // Assert
        Assert.False(versionsResult.IsError);
        List<School>? versions = versionsResult.Value;
        Assert.NotNull(versions);
        Assert.Equal(2, versions.Count);

        Assert.Equal("School 1", versions[0].Name);
        Assert.Equal("School 2", versions[1].Name);
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
