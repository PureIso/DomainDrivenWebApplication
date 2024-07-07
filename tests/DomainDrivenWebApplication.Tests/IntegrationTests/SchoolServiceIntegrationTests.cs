using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Domain.Services;
using DomainDrivenWebApplication.Tests.Fixtures;

namespace DomainDrivenWebApplication.Tests.IntegrationTests;

[Collection("SchoolServiceTests")]
public class SchoolServiceIntegrationTests : IClassFixture<SchoolFixture> , IDisposable, IAsyncLifetime
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
        bool inserted = await service.AddSchoolAsync(school);
        Assert.True(inserted);
        School? retrievedSchool = await service.GetSchoolByIdAsync(school.Id);

        // Assert
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

        bool inserted = await service.AddSchoolAsync(school1);
        Assert.True(inserted);
        inserted = await service.AddSchoolAsync(school2);
        Assert.True(inserted);

        // Act
        List<School>? schools = await service.GetAllSchoolsAsync();

        // Assert
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
        bool inserted = await service.AddSchoolAsync(school);
        Assert.True(inserted);

        // Act
        school.Name = "Updated School";
        await service.UpdateSchoolAsync(school);
        School? updatedSchool = await service.GetSchoolByIdAsync(school.Id);

        // Assert
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
        await service.AddSchoolAsync(school);

        // Act
        await service.DeleteSchoolAsync(school.Id);
        School? deletedSchool = await service.GetSchoolByIdAsync(school.Id);

        // Assert
        Assert.Null(deletedSchool);
    }

    [Fact]
    public async Task Can_Get_Schools_By_Date_Range()
    {
        // Arrange
        SchoolService? service = _fixture.SchoolService;
        Assert.NotNull(service);

        School school = new School { Name = "School 1", Address = "Test Address", PrincipalName = "John Doe"};
        bool inserted = await service.AddSchoolAsync(school);
        Assert.True(inserted);

        DateTime fromDate = DateTime.UtcNow;
        School? retrievedSchool = await service.GetSchoolByIdAsync(1);
        Assert.NotNull(retrievedSchool);

        retrievedSchool.Name = "School 2";
        await service.UpdateSchoolAsync(retrievedSchool);

        // Act
        List<School>? schools = await service.GetSchoolsByDateRangeAsync(fromDate.AddDays(5), DateTime.MaxValue);


        // Assert
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
        School school = new School { Name = "School 1", Address = "Test Address", PrincipalName = "John Doe"};

        bool inserted = await service.AddSchoolAsync(school);
        Assert.True(inserted);

        // Update the school to create a new version
        School? retrievedSchool = await service.GetSchoolByIdAsync(1);
        Assert.NotNull(retrievedSchool);
        retrievedSchool.Name = "School 2";
        retrievedSchool.Address = "Address 2";

        await service.UpdateSchoolAsync(retrievedSchool);


        // Act
        List<School>? schools = await service.GetAllVersionsOfSchoolAsync(1);

        // Assert
        Assert.NotNull(schools);
        Assert.Equal(2, schools.Count); // Expecting two versions

        // Optional: Assert specific properties of each version if needed
        Assert.Equal("School 1", schools[0].Name);
        Assert.Equal("School 2", schools[1].Name);
    }


    private async Task CleanupData()
    {
        if (_fixture.SchoolService != null)
        {
            List<School>? schools = await _fixture.SchoolService.GetAllSchoolsAsync();
            if (schools != null)
            {
                foreach (School school in schools)
                {
                    await _fixture.SchoolService.DeleteSchoolAsync(school.Id);
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