using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Domain.Interfaces;
using DomainDrivenWebApplication.Domain.Services;
using ErrorOr;
using Moq;

namespace DomainDrivenWebApplication.Tests.UnitTests;

public class SchoolServiceTests
{
    private readonly Mock<ISchoolCommandRepository> _mockSchoolCommandRepository;
    private readonly Mock<ISchoolQueryRepository> _mockSchoolQueryRepository;
    private readonly SchoolServiceCommandQuery _schoolService;

    public SchoolServiceTests()
    {
        _mockSchoolCommandRepository = new Mock<ISchoolCommandRepository>();
        _mockSchoolQueryRepository = new Mock<ISchoolQueryRepository>();
        _schoolService = new SchoolServiceCommandQuery(_mockSchoolCommandRepository.Object, _mockSchoolQueryRepository.Object);
    }

    [Fact]
    public async Task GetSchoolByIdAsync_ReturnsSchool()
    {
        // Arrange
        int schoolId = 1;
        School school = new School { Id = schoolId, Name = "School 1" };
        _mockSchoolQueryRepository
            .Setup(repo => repo.GetByIdAsync(schoolId))
            .ReturnsAsync(school);

        // Act
        ErrorOr<School> result = await _schoolService.GetSchoolByIdAsync(schoolId);

        // Assert
        Assert.False(result.IsError, "Expected no errors when retrieving school.");
        Assert.Equal(schoolId, result.Value.Id);
        Assert.Equal("School 1", result.Value.Name);
    }

    [Fact]
    public async Task GetAllSchoolsAsync_ReturnsListOfSchools()
    {
        // Arrange
        List<School> schools = new List<School>
        {
            new School { Id = 1, Name = "School 1" },
            new School { Id = 2, Name = "School 2" }
        };
        _mockSchoolQueryRepository
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(schools);

        // Act
        ErrorOr<List<School>> result = await _schoolService.GetAllSchoolsAsync();

        // Assert
        Assert.False(result.IsError, "Expected no errors when retrieving all schools.");
        Assert.Equal(schools.Count, result.Value.Count);
    }

    [Fact]
    public async Task AddSchoolAsync_AddsNewSchool()
    {
        // Arrange
        School newSchool = new School { Name = "New School" };
        _mockSchoolCommandRepository
            .Setup(repo => repo.AddAsync(newSchool))
            .ReturnsAsync(true);

        // Act
        ErrorOr<bool> result = await _schoolService.AddSchoolAsync(newSchool);

        // Assert
        Assert.False(result.IsError, "Expected no errors when adding school.");
        Assert.True(result.Value);
        _mockSchoolCommandRepository.Verify(repo => repo.AddAsync(newSchool), Times.Once);
    }

    [Fact]
    public async Task UpdateSchoolAsync_UpdatesSchool()
    {
        // Arrange
        School existingSchool = new School { Id = 1, Name = "Existing School" };
        _mockSchoolCommandRepository
            .Setup(repo => repo.UpdateAsync(existingSchool))
            .ReturnsAsync(true);

        // Act
        ErrorOr<bool> result = await _schoolService.UpdateSchoolAsync(existingSchool);

        // Assert
        Assert.False(result.IsError, "Expected no errors when updating school.");
        Assert.True(result.Value);
        _mockSchoolCommandRepository.Verify(repo => repo.UpdateAsync(existingSchool), Times.Once);
    }

    [Fact]
    public async Task DeleteSchoolAsync_DeletesSchool()
    {
        // Arrange
        int schoolId = 1;
        School existingSchool = new School { Id = schoolId, Name = "Existing School" };
        _mockSchoolQueryRepository
            .Setup(repo => repo.GetByIdAsync(schoolId))
            .ReturnsAsync(existingSchool);
        _mockSchoolCommandRepository
            .Setup(repo => repo.DeleteAsync(existingSchool))
            .ReturnsAsync(true);

        // Act
        ErrorOr<bool> result = await _schoolService.DeleteSchoolAsync(schoolId);

        // Assert
        Assert.False(result.IsError, "Expected no errors when deleting school.");
        Assert.True(result.Value);
        _mockSchoolCommandRepository.Verify(repo => repo.DeleteAsync(existingSchool), Times.Once);
    }

    [Fact]
    public async Task GetSchoolsByDateRangeAsync_ReturnsSchoolsInDateRange()
    {
        // Arrange
        DateTime fromDate = DateTime.Parse("2023-01-01");
        DateTime toDate = DateTime.Parse("2023-12-31");
        List<School> schoolsInRange = new List<School>
        {
            new School { Id = 1, Name = "School 1" },
            new School { Id = 2, Name = "School 2" }
        };
        _mockSchoolQueryRepository
            .Setup(repo => repo.GetSchoolsByDateRangeAsync(fromDate, toDate))
            .ReturnsAsync(schoolsInRange);

        // Act
        ErrorOr<List<School>> result = await _schoolService.GetSchoolsByDateRangeAsync(fromDate, toDate);

        // Assert
        Assert.False(result.IsError, "Expected no errors when retrieving schools by date range.");
        Assert.Equal(schoolsInRange.Count, result.Value.Count);
    }

    [Fact]
    public async Task GetAllVersionsOfSchoolAsync_ReturnsVersionsOfSchool()
    {
        // Arrange
        int schoolId = 1;
        List<School> schoolVersions = new List<School>
        {
            new School { Id = schoolId, Name = "School 1" },
            new School { Id = schoolId, Name = "School 1 - Updated" }
        };
        _mockSchoolQueryRepository
            .Setup(repo => repo.GetAllVersionsAsync(schoolId))
            .ReturnsAsync(schoolVersions);

        // Act
        ErrorOr<List<School>> result = await _schoolService.GetAllVersionsOfSchoolAsync(schoolId);

        // Assert
        Assert.False(result.IsError, "Expected no errors when retrieving all versions of a school.");
        Assert.Equal(schoolVersions.Count, result.Value.Count);
    }
}
