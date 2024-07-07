using AutoMapper;
using DomainDrivenWebApplication.API.Controllers;
using DomainDrivenWebApplication.API.Models;
using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Domain.Interfaces;
using DomainDrivenWebApplication.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace DomainDrivenWebApplication.Tests.UnitTests
{
    public class SchoolControllerTests
    {
        private readonly Mock<ISchoolRepository> _mockSchoolRepository;
        private readonly SchoolService _schoolService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<SchoolController>> _mockLogger;
        private readonly SchoolController _schoolController;

        public SchoolControllerTests()
        {
            _mockSchoolRepository = new Mock<ISchoolRepository>();
            _schoolService = new SchoolService(_mockSchoolRepository.Object);
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<SchoolController>>();
            _schoolController = new SchoolController(_schoolService, _mockMapper.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllSchools_ReturnsOkWithSchoolDtos()
        {
            // Arrange
            List<School> schools = new List<School>
            {
                new School { Id = 1, Name = "School 1", Address = "Address 1", CreatedAt = DateTime.UtcNow },
                new School { Id = 2, Name = "School 2", Address = "Address 2", CreatedAt = DateTime.UtcNow }
            };
            List<SchoolDto> schoolDtos = new List<SchoolDto>
            {
                new SchoolDto { Id = 1, Name = "School 1", Address = "Address 1", CreatedAt = DateTime.UtcNow },
                new SchoolDto { Id = 2, Name = "School 2", Address = "Address 2", CreatedAt = DateTime.UtcNow }
            };
            _mockSchoolRepository.Setup(s => s.GetAllAsync()).ReturnsAsync(schools);
            _mockMapper.Setup(m => m.Map<List<SchoolDto>>(schools)).Returns(schoolDtos);

            // Act
            ActionResult<List<SchoolDto>> result = await _schoolController.GetAllSchools();

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            List<SchoolDto> returnedSchoolDtos = Assert.IsAssignableFrom<List<SchoolDto>>(okResult.Value);
            Assert.Equal(schoolDtos.Count, returnedSchoolDtos.Count);

            // Additional assertions to compare individual elements if needed
            for (int i = 0; i < schoolDtos.Count; i++)
            {
                Assert.Equal(schoolDtos[i].Id, returnedSchoolDtos[i].Id);
                Assert.Equal(schoolDtos[i].Name, returnedSchoolDtos[i].Name);
                Assert.Equal(schoolDtos[i].Address, returnedSchoolDtos[i].Address);
                Assert.Equal(schoolDtos[i].CreatedAt, returnedSchoolDtos[i].CreatedAt);
            }

            // Verify that the mock methods were called as expected
            _mockSchoolRepository.Verify(s => s.GetAllAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map<List<SchoolDto>>(schools), Times.Once);
        }
    }
}
