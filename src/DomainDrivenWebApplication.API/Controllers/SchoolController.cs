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
    [HttpGet]
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
    [HttpGet("{id}")]
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
    [HttpPost]
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
    [HttpPut("{id}")]
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
    [HttpDelete("{id}")]
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
    [HttpGet("history/{id}")]
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
    [HttpGet("range")]
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
