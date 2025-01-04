using Asp.Versioning;
using AutoMapper;
using DomainDrivenWebApplication.API.Middleware;
using DomainDrivenWebApplication.Domain.Common.Models;
using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Domain.Services;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DomainDrivenWebApplication.API.Controllers;

/// <summary>
/// Controller for managing School entities, providing endpoints for retrieving, adding, updating, and deleting schools.
/// Inherits from <see cref="BaseController"/> for common error handling functionality.
/// </summary>
[ServiceFilter(typeof(ServiceTypeFilter))] 
[ApiController]
[Route("api/v{version:apiVersion}/school")]
[ApiVersion("1.0")]
public class SchoolController : BaseController
{
    private readonly SchoolServiceCommandQuery _schoolService;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchoolController"/> class.
    /// </summary>
    /// <param name="schoolService">The <see cref="SchoolServiceCommandQuery"/> for interacting with school data.</param>
    /// <param name="mapper">The <see cref="IMapper"/> to map between entities and DTOs.</param>
    /// <param name="logger">The <see cref="ILogger{SchoolController}"/> for logging errors and events.</param>
    /// <param name="localizer">The <see cref="IStringLocalizer{BaseController}"/> for localizing error messages.</param>
    public SchoolController(SchoolServiceCommandQuery schoolService, IMapper mapper, ILogger<SchoolController> logger, IStringLocalizer<BaseController> localizer)
        : base(logger, localizer)
    {
        _schoolService = schoolService;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves all schools from the database.
    /// </summary>
    /// <returns>A list of <see cref="SchoolDto"/> representing all schools.</returns>
    /// <response code="200">Returns a list of schools.</response>
    /// <response code="500">If an error occurs while retrieving the schools.</response>
    [HttpGet]
    [ApiExplorerSettings(GroupName = "reader")]
    [ProducesResponseType(typeof(List<SchoolDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllSchools()
    {
        try
        {
            ErrorOr<List<School>> result = await _schoolService.GetAllSchoolsAsync();
            return result.Match(
                success => Ok(_mapper.Map<List<SchoolDto>>(success)),
                HandleErrors
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error occurred while getting all schools");
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }

    /// <summary>
    /// Retrieves a specific school by its ID.
    /// </summary>
    /// <param name="id">The ID of the school to retrieve.</param>
    /// <returns>A <see cref="SchoolDto"/> representing the school, or a 404 if not found.</returns>
    /// <response code="200">Returns the school with the specified ID.</response>
    /// <response code="404">If the school with the specified ID is not found.</response>
    /// <response code="500">If an error occurs while retrieving the school.</response>
    [HttpGet("{id}")]
    [ApiExplorerSettings(GroupName = "reader")]
    [ProducesResponseType(typeof(SchoolDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetSchoolById(int id)
    {
        try
        {
            ErrorOr<School> result = await _schoolService.GetSchoolByIdAsync(id);
            IActionResult a = result.Match(
                success => Ok(_mapper.Map<SchoolDto>(success)),
                HandleErrors
            );
            return a;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error occurred while getting school by id {SchoolId}", id);
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }

    /// <summary>
    /// Adds a new school to the database.
    /// </summary>
    /// <param name="schoolDto">The data transfer object containing the school information to add.</param>
    /// <returns>A 201 response with the created school.</returns>
    /// <response code="201">Returns the newly created school.</response>
    /// <response code="400">If the provided school data is invalid.</response>
    /// <response code="500">If an error occurs while adding the school.</response>
    [HttpPost]
    [ApiExplorerSettings(GroupName = "writer")]
    [ProducesResponseType(typeof(SchoolDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> AddSchool([FromBody] SchoolDto schoolDto)
    {
        try
        {
            if (schoolDto == null)
            {
                return BadRequest("School data is required.");
            }

            School school = _mapper.Map<School>(schoolDto);
            ErrorOr<bool> result = await _schoolService.AddSchoolAsync(school);
            return result.Match(
                success => CreatedAtAction(nameof(GetSchoolById), new { id = school.Id }, _mapper.Map<SchoolDto>(school)),
                HandleErrors
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error occurred while adding a new school");
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }

    /// <summary>
    /// Updates an existing school by its ID.
    /// </summary>
    /// <param name="id">The ID of the school to update.</param>
    /// <param name="schoolDto">The data transfer object containing the updated school information.</param>
    /// <returns>A 204 response if the update is successful.</returns>
    /// <response code="204">Indicates that the school was successfully updated.</response>
    /// <response code="400">If the provided school data is invalid or the ID does not match.</response>
    /// <response code="500">If an error occurs while updating the school.</response>
    [HttpPut("{id}")]
    [ApiExplorerSettings(GroupName = "writer")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdateSchool(int id, [FromBody] SchoolDto schoolDto)
    {
        try
        {
            if (id != schoolDto.Id)
            {
                return BadRequest("School ID mismatch.");
            }

            School school = _mapper.Map<School>(schoolDto);
            ErrorOr<bool> result = await _schoolService.UpdateSchoolAsync(school);
            return result.Match(
                success => NoContent(),
                HandleErrors
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error occurred while updating school with id {SchoolId}", id);
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }

    /// <summary>
    /// Deletes a school by its ID.
    /// </summary>
    /// <param name="id">The ID of the school to delete.</param>
    /// <returns>A 204 response if the school was successfully deleted.</returns>
    /// <response code="204">Indicates that the school was successfully deleted.</response>
    /// <response code="404">If the school with the specified ID is not found.</response>
    /// <response code="500">If an error occurs while deleting the school.</response>
    [HttpDelete("{id}")]
    [ApiExplorerSettings(GroupName = "writer")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteSchool(int id)
    {
        try
        {
            ErrorOr<School> result = await _schoolService.GetSchoolByIdAsync(id);
            if (result.IsError)
            {
                return NotFound();
            }

            await _schoolService.DeleteSchoolAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error occurred while deleting school with id {SchoolId}", id);
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }

    /// <summary>
    /// Retrieves a list of schools created within a specific date range.
    /// </summary>
    /// <param name="fromDate">The start date for filtering schools.</param>
    /// <param name="toDate">The end date for filtering schools.</param>
    /// <returns>A list of <see cref="SchoolDto"/> representing schools within the specified date range.</returns>
    /// <response code="200">Returns a list of schools created within the specified date range.</response>
    /// <response code="400">If the date range is invalid.</response>
    /// <response code="500">If an error occurs while retrieving the schools.</response>
    [HttpGet("by-date-range")]
    [ApiExplorerSettings(GroupName = "reader")]
    [ProducesResponseType(typeof(List<SchoolDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetSchoolsByDateRange([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        try
        {
            if (fromDate > toDate)
            {
                return BadRequest("The 'from' date must not be later than the 'to' date.");
            }

            // Call the service method to get schools within the date range
            ErrorOr<List<School>> result = await _schoolService.GetSchoolsByDateRangeAsync(fromDate, toDate);

            return result.Match(
                success => Ok(_mapper.Map<List<SchoolDto>>(success)),
                HandleErrors
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error occurred while retrieving schools by date range.");
            return Problem(detail: ex.Message, statusCode: 500);
        }
    }
}
