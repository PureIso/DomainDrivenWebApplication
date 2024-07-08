using AutoMapper;
using DomainDrivenWebApplication.API.Models;
using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace DomainDrivenWebApplication.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SchoolController : ControllerBase
{
    private readonly SchoolService _schoolService;
    private readonly IMapper _mapper;
    private readonly ILogger<SchoolController>? _logger;

    public SchoolController(SchoolService schoolService, IMapper mapper, ILogger<SchoolController>? logger)
    {
        _schoolService = schoolService;
        _mapper = mapper;
        _logger = logger;
    }

    // GET: api/school
    /// <summary>
    /// Retrieves all schools.
    /// </summary>
    /// <returns>A list of SchoolDto objects.</returns>
    /// <response code="200">Returns the list of schools.</response>
    /// <response code="500">If an unexpected error occurs.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<SchoolDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<List<SchoolDto>>> GetAllSchools()
    {
        try
        {
            List<School>? schools = await _schoolService.GetAllSchoolsAsync();
            List<SchoolDto> schoolDtos = _mapper.Map<List<SchoolDto>>(schools);
            return Ok(schoolDtos);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error occurred while getting all schools");
            return Problem(detail: ex.Message, statusCode: 500); // Internal Server Error
        }
    }

    // GET: api/school/5
    /// <summary>
    /// Retrieves a school by ID.
    /// </summary>
    /// <param name="id">The ID of the school.</param>
    /// <returns>A SchoolDto object.</returns>
    /// <response code="200">Returns the school with the specified ID.</response>
    /// <response code="404">If the school with the given ID does not exist.</response>
    /// <response code="500">If an unexpected error occurs.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SchoolDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<SchoolDto>> GetSchoolById(int id)
    {
        try
        {
            School? school = await _schoolService.GetSchoolByIdAsync(id);
            if (school == null)
            {
                return NotFound(); // 404 Not Found
            }
            SchoolDto schoolDto = _mapper.Map<SchoolDto>(school);
            return Ok(schoolDto); // 200 OK
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error occurred while getting school by id {SchoolId}", id);
            return Problem(detail: ex.Message, statusCode: 500); // Internal Server Error
        }
    }

    // POST: api/school
    /// <summary>
    /// Adds a new school.
    /// </summary>
    /// <param name="schoolDto">The SchoolDto object representing the school to add.</param>
    /// <returns>The newly created SchoolDto object.</returns>
    /// <response code="201">Returns the newly created school.</response>
    /// <response code="400">If the schoolDto is invalid or null.</response>
    /// <response code="500">If an unexpected error occurs.</response>
    [HttpPost]
    [ProducesResponseType(typeof(SchoolDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<SchoolDto>> AddSchool([FromBody] SchoolDto schoolDto)
    {
        try
        {
            School school = _mapper.Map<School>(schoolDto);
            await _schoolService.AddSchoolAsync(school);
            SchoolDto addedSchoolDto = _mapper.Map<SchoolDto>(school);
            return CreatedAtAction(nameof(GetSchoolById), new { id = school.Id }, addedSchoolDto); // 201 Created
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error occurred while adding a new school");
            return Problem(detail: ex.Message, statusCode: 500); // Internal Server Error
        }
    }

    // PUT: api/school/5
    /// <summary>
    /// Updates an existing school.
    /// </summary>
    /// <param name="id">The ID of the school to update.</param>
    /// <param name="schoolDto">The updated SchoolDto object.</param>
    /// <returns>NoContent if successful, BadRequest if IDs do not match.</returns>
    /// <response code="204">If the school was successfully updated.</response>
    /// <response code="400">If the ID in the path does not match the ID in the schoolDto.</response>
    /// <response code="500">If an unexpected error occurs.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdateSchool(int id, [FromBody] SchoolDto schoolDto)
    {
        try
        {
            School school = _mapper.Map<School>(schoolDto);
            if (id != school.Id)
            {
                return BadRequest(); // 400 Bad Request
            }

            await _schoolService.UpdateSchoolAsync(school);
            return NoContent(); // 204 No Content
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error occurred while updating school with id {SchoolId}", id);
            return Problem(detail: ex.Message, statusCode: 500); // Internal Server Error
        }
    }

    // DELETE: api/school/5
    /// <summary>
    /// Deletes a school by ID.
    /// </summary>
    /// <param name="id">The ID of the school to delete.</param>
    /// <returns>NoContent if successful, NotFound if school not found.</returns>
    /// <response code="204">If the school was successfully deleted.</response>
    /// <response code="404">If the school with the given ID does not exist.</response>
    /// <response code="500">If an unexpected error occurs.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteSchool(int id)
    {
        try
        {
            School? school = await _schoolService.GetSchoolByIdAsync(id);
            if (school == null)
            {
                return NotFound(); // 404 Not Found
            }

            await _schoolService.DeleteSchoolAsync(id);
            return NoContent(); // 204 No Content
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error occurred while deleting school with id {SchoolId}", id);
            return Problem(detail: ex.Message, statusCode: 500); // Internal Server Error
        }
    }

    // GET: api/school/history/5
    /// <summary>
    /// Retrieves the history of changes for a school by ID.
    /// </summary>
    /// <param name="id">The ID of the school.</param>
    /// <returns>A list of SchoolDto objects representing the history.</returns>
    /// <response code="200">Returns the history of changes for the school.</response>
    /// <response code="404">If the school with the given ID does not exist.</response>
    /// <response code="500">If an unexpected error occurs.</response>
    [HttpGet("history/{id}")]
    [ProducesResponseType(typeof(List<SchoolDto>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<List<SchoolDto>>> GetSchoolHistory(int id)
    {
        try
        {
            List<School>? history = await _schoolService.GetAllVersionsOfSchoolAsync(id);
            if (history == null || history.Count == 0)
            {
                return NotFound(); // 404 Not Found
            }
            List<SchoolDto> historyDtos = _mapper.Map<List<SchoolDto>>(history);
            return Ok(historyDtos); // 200 OK
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error occurred while getting history for school with id {SchoolId}", id);
            return Problem(detail: ex.Message, statusCode: 500); // Internal Server Error
        }
    }

    // GET: api/school/range?fromDate=2023-01-01&toDate=2023-12-31
    /// <summary>
    /// Retrieves schools within a specified date range.
    /// </summary>
    /// <param name="fromDate">The start date of the range.</param>
    /// <param name="toDate">The end date of the range.</param>
    /// <returns>A list of SchoolDto objects within the specified date range.</returns>
    /// <response code="200">Returns the schools within the specified date range.</response>
    /// <response code="404">If no schools are found within the specified date range.</response>
    /// <response code="500">If an unexpected error occurs.</response>
    [HttpGet("range")]
    [ProducesResponseType(typeof(List<SchoolDto>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<List<SchoolDto>>> GetSchoolsByDateRange([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        try
        {
            List<School>? schools = await _schoolService.GetSchoolsByDateRangeAsync(fromDate, toDate);
            if (schools == null || schools.Count == 0)
            {
                return NotFound(); // 404 Not Found
            }
            List<SchoolDto> schoolDtos = _mapper.Map<List<SchoolDto>>(schools);
            return Ok(schoolDtos); // 200 OK
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "An error occurred while getting schools by date range from {FromDate} to {ToDate}", fromDate, toDate);
            return Problem(detail: ex.Message, statusCode: 500); // Internal Server Error
        }
    }
}