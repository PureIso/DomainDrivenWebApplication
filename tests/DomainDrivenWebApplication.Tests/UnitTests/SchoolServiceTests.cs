using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Domain.Interfaces;
using DomainDrivenWebApplication.Domain.Services;
using Moq;

namespace DomainDrivenWebApplication.Tests.UnitTests
{
    public class SchoolServiceTests
    {
        private readonly Mock<ISchoolRepository> _mockSchoolRepository;
        private readonly SchoolService _schoolService;

        public SchoolServiceTests()
        {
            _mockSchoolRepository = new Mock<ISchoolRepository>();
            _schoolService = new SchoolService(_mockSchoolRepository.Object);
        }

        [Fact]
        public async Task GetSchoolByIdAsync_ReturnsSchool()
        {
            // Arrange
            int schoolId = 1;
            School school = new School { Id = schoolId, Name = "School 1" };
            _mockSchoolRepository.Setup(repo => repo.GetByIdAsync(schoolId)).ReturnsAsync(school);

            // Act
            School? result = await _schoolService.GetSchoolByIdAsync(schoolId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(schoolId, result.Id);
            Assert.Equal("School 1", result.Name);
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
            _mockSchoolRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(schools);

            // Act
            List<School>? result = await _schoolService.GetAllSchoolsAsync();

            // Assert
            Assert.Equal(schools.Count, result.Count);
        }

        [Fact]
        public async Task AddSchoolAsync_AddsNewSchool()
        {
            // Arrange
            School newSchool = new School { Name = "New School" };

            // Act
            await _schoolService.AddSchoolAsync(newSchool);

            // Assert
            _mockSchoolRepository.Verify(repo => repo.AddAsync(newSchool), Times.Once);
        }

        [Fact]
        public async Task UpdateSchoolAsync_UpdatesSchool()
        {
            // Arrange
            School existingSchool = new School { Id = 1, Name = "Existing School" };

            // Act
            await _schoolService.UpdateSchoolAsync(existingSchool);

            // Assert
            _mockSchoolRepository.Verify(repo => repo.UpdateAsync(existingSchool), Times.Once);
        }

        [Fact]
        public async Task DeleteSchoolAsync_DeletesSchool()
        {
            // Arrange
            int schoolId = 1;
            School existingSchool = new School { Id = schoolId, Name = "Existing School" };
            _mockSchoolRepository.Setup(repo => repo.GetByIdAsync(schoolId)).ReturnsAsync(existingSchool);

            // Act
            await _schoolService.DeleteSchoolAsync(schoolId);

            // Assert
            _mockSchoolRepository.Verify(repo => repo.DeleteAsync(existingSchool), Times.Once);
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
            _mockSchoolRepository.Setup(repo => repo.GetSchoolsByDateRangeAsync(fromDate, toDate)).ReturnsAsync(schoolsInRange);

            // Act
            List<School>? result = await _schoolService.GetSchoolsByDateRangeAsync(fromDate, toDate);

            // Assert
            Assert.Equal(schoolsInRange.Count, result.Count);
        }

        [Fact]
        public async Task GetAllVersionsOfSchoolAsync_ReturnsVersionsOfSchool()
        {
            // Arrange
            int schoolId = 1;
            List<School> schoolVersions = new List<School>
            {
                new School { Id = schoolId, Name = "School 1", 
                    //ValidFrom = DateTime.Parse("2023-01-01"), ValidTo = DateTime.Parse("2023-06-30")
                },
                new School { Id = schoolId, Name = "School 1", 
                    //ValidFrom = DateTime.Parse("2023-06-30"), ValidTo = DateTime.Parse("9999-12-31")
                }
            };
            _mockSchoolRepository.Setup(repo => repo.GetAllVersionsAsync(schoolId)).ReturnsAsync(schoolVersions);

            // Act
            List<School>? result = await _schoolService.GetAllVersionsOfSchoolAsync(schoolId);

            // Assert
            Assert.Equal(schoolVersions.Count, result.Count);
        }
    }
}
