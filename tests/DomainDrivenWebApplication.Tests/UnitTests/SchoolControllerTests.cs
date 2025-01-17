using AutoMapper;
using DomainDrivenWebApplication.API.Controllers;
using DomainDrivenWebApplication.Domain.Common.Models;
using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Domain.Interfaces;
using DomainDrivenWebApplication.Domain.Services;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace DomainDrivenWebApplication.Tests.UnitTests;

public class SchoolControllerTests
{
    private readonly Mock<ISchoolCommandRepository> _mockSchoolCommandRepository;
    private readonly Mock<ISchoolQueryRepository> _mockSchoolQueryRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IStringLocalizer<BaseController>> _mockLocalizer;
    private readonly SchoolController _schoolController;

    public SchoolControllerTests()
    {
        _mockSchoolCommandRepository = new Mock<ISchoolCommandRepository>();
        _mockSchoolQueryRepository = new Mock<ISchoolQueryRepository>();
        SchoolServiceCommandQuery schoolService = new(_mockSchoolCommandRepository.Object, _mockSchoolQueryRepository.Object);
        _mockMapper = new Mock<IMapper>();
        Mock<ILogger<SchoolController>> mockLogger = new();
        _mockLocalizer = new Mock<IStringLocalizer<BaseController>>();

        _schoolController = new SchoolController(
            schoolService,
            _mockMapper.Object,
            mockLogger.Object,
            _mockLocalizer.Object
        );
    }

    [Fact]
    public async Task AddSchool_ReturnsCreatedResult_WhenSchoolIsAddedSuccessfully()
    {
        // Arrange
        SchoolDto schoolDto = new SchoolDto { Name = "New School", Address = "New Address", CreatedAt = DateTime.UtcNow };
        School school = new School { Id = 1, Name = "New School", Address = "New Address", CreatedAt = DateTime.UtcNow };

        _mockMapper.Setup(mapper => mapper.Map<School>(schoolDto)).Returns(school);
        _mockSchoolCommandRepository.Setup(repo => repo.AddAsync(school)).ReturnsAsync(true);
        _mockMapper.Setup(mapper => mapper.Map<SchoolDto>(school)).Returns(new SchoolDto { Id = 1, Name = "New School", Address = "New Address" });

        // Act
        IActionResult result = await _schoolController.AddSchool(schoolDto);

        // Assert
        CreatedAtActionResult createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
        SchoolDto returnedSchoolDto = Assert.IsType<SchoolDto>(createdAtResult.Value);
        Assert.Equal(school.Id, returnedSchoolDto.Id);
    }

    [Fact]
    public async Task AddSchool_ReturnsBadRequest_WhenSchoolCreationFails()
    {
        SchoolDto schoolDto = new SchoolDto { Name = "New School", Address = "New Address", CreatedAt = DateTime.UtcNow };
        School school = new School { Id = 1, Name = "New School", Address = "New Address", CreatedAt = DateTime.UtcNow };

        _mockLocalizer
            .Setup(localizer => localizer["FailedToAddSchool"])
            .Returns(new LocalizedString("FailedToAddSchool", "Failed to add the school."));
        ErrorOr<bool> failureResult = Error.Failure(
            code: "FailedToAddSchool",
            description: _mockLocalizer.Object["FailedToAddSchool"].Value);

        _mockMapper.Setup(mapper => mapper.Map<School>(schoolDto)).Returns(school);
        _mockSchoolCommandRepository.Setup(repo => repo.AddAsync(school)).ReturnsAsync(failureResult);

        IActionResult result = await _schoolController.AddSchool(schoolDto);

        ObjectResult notFoundResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, notFoundResult.StatusCode);

        ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(notFoundResult.Value);
        Assert.Equal("FailedToAddSchool", problemDetails.Title);
        Assert.Equal("Failed to add the school.", problemDetails.Detail);
        Assert.Equal(400, problemDetails.Status);
    }

    [Fact]
    public async Task GetSchoolById_ReturnsNotFound_WhenSchoolDoesNotExist()
    {
        int schoolId = 1;
        _mockLocalizer
            .Setup(localizer => localizer["SchoolNotFound"])
            .Returns(new LocalizedString("SchoolNotFound", "The specified school could not be found."));

        ErrorOr<School> failureResult = Error.NotFound(
            code: "SchoolNotFound",
            description: _mockLocalizer.Object["SchoolNotFound"].Value);

        _mockSchoolQueryRepository.Setup(repo => repo.GetByIdAsync(schoolId)).ReturnsAsync(failureResult);

        IActionResult result = await _schoolController.GetSchoolById(schoolId);

        ObjectResult notFoundResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);

        ProblemDetails problemDetails = Assert.IsType<ProblemDetails>(notFoundResult.Value);
        Assert.Equal("SchoolNotFound", problemDetails.Title);
        Assert.Equal("The specified school could not be found.", problemDetails.Detail);
        Assert.Equal(404, problemDetails.Status);
    }

    [Fact]
    public async Task GetSchoolById_ReturnsOkWithSchoolDto_WhenSchoolExists()
    {
        int schoolId = 1;
        School school = new School { Id = schoolId, Name = "Test School", Address = "Test Address", CreatedAt = DateTime.UtcNow };
        SchoolDto schoolDto = new SchoolDto { Id = schoolId, Name = "Test School", Address = "Test Address", CreatedAt = DateTime.UtcNow };

        _mockSchoolQueryRepository.Setup(repo => repo.GetByIdAsync(schoolId)).ReturnsAsync(school);
        _mockMapper.Setup(mapper => mapper.Map<SchoolDto>(school)).Returns(schoolDto);

        IActionResult result = await _schoolController.GetSchoolById(schoolId);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        SchoolDto returnedSchoolDto = Assert.IsType<SchoolDto>(okResult.Value);
        Assert.Equal(schoolDto.Id, returnedSchoolDto.Id);
    }

    [Fact]
    public async Task GetAllSchools_ReturnsOkWithSchoolDtos()
    {
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

        _mockSchoolQueryRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(schools);
        _mockMapper.Setup(mapper => mapper.Map<List<SchoolDto>>(schools)).Returns(schoolDtos);

        IActionResult result = await _schoolController.GetAllSchools();

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        List<SchoolDto> returnedSchoolDtos = Assert.IsType<List<SchoolDto>>(okResult.Value);
        Assert.Equal(schoolDtos.Count, returnedSchoolDtos.Count);
    }

    [Fact]
    public async Task GetSchoolsByDateRange_ReturnsBadRequest_WhenFromDateIsLaterThanToDate()
    {
        DateTime fromDate = DateTime.UtcNow.AddDays(1);
        DateTime toDate = DateTime.UtcNow;

        IActionResult result = await _schoolController.GetSchoolsByDateRange(fromDate, toDate);

        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("The 'from' date must not be later than the 'to' date.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetSchoolsByDateRange_ReturnsOkWithSchoolDtos_WhenValidDates()
    {
        DateTime fromDate = DateTime.UtcNow.AddDays(-30);
        DateTime toDate = DateTime.UtcNow;
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

        _mockSchoolQueryRepository.Setup(repo => repo.GetSchoolsByDateRangeAsync(fromDate, toDate)).ReturnsAsync(schools);
        _mockMapper.Setup(mapper => mapper.Map<List<SchoolDto>>(schools)).Returns(schoolDtos);

        IActionResult result = await _schoolController.GetSchoolsByDateRange(fromDate, toDate);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        List<SchoolDto> returnedSchoolDtos = Assert.IsType<List<SchoolDto>>(okResult.Value);
        Assert.Equal(schoolDtos.Count, returnedSchoolDtos.Count);
    }
}
