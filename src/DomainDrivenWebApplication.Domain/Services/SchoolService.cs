using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Domain.Interfaces;
using ErrorOr;

namespace DomainDrivenWebApplication.Domain.Services;

/// <summary>
/// Service for managing school-related operations.
/// </summary>
public class SchoolService
{
    private readonly ISchoolRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchoolService"/> class.
    /// </summary>
    /// <param name="repository">The school repository instance.</param>
    public SchoolService(ISchoolRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Retrieves a school by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the school.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing the school entity or errors.</returns>
    public async Task<ErrorOr<School>> GetSchoolByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    /// <summary>
    /// Retrieves all schools.
    /// </summary>
    /// <returns>An <see cref="ErrorOr{T}"/> containing a list of schools or errors.</returns>
    public async Task<ErrorOr<List<School>>> GetAllSchoolsAsync()
    {
        return await _repository.GetAllAsync();
    }

    /// <summary>
    /// Adds a new school.
    /// </summary>
    /// <param name="school">The school entity to add.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> indicating success or containing errors.</returns>
    public async Task<ErrorOr<bool>> AddSchoolAsync(School school)
    {
        school.CreatedAt = DateTime.UtcNow;
        school.PrincipalName = string.IsNullOrWhiteSpace(school.PrincipalName)
            ? "Default Principal"
            : school.PrincipalName;

        return await _repository.AddAsync(school);
    }

    /// <summary>
    /// Updates an existing school.
    /// </summary>
    /// <param name="school">The school entity to update.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> indicating success or containing errors.</returns>
    public async Task<ErrorOr<bool>> UpdateSchoolAsync(School school)
    {
        return await _repository.UpdateAsync(school);
    }

    /// <summary>
    /// Deletes a school by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the school to delete.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> indicating success or containing errors.</returns>
    public async Task<ErrorOr<bool>> DeleteSchoolAsync(int id)
    {
        ErrorOr<School> schoolResult = await _repository.GetByIdAsync(id);
        if (schoolResult.IsError)
        {
            return schoolResult.Errors;
        }
        return await _repository.DeleteAsync(schoolResult.Value);
    }

    /// <summary>
    /// Retrieves schools created within a specified date range.
    /// </summary>
    /// <param name="fromDate">The start date of the range.</param>
    /// <param name="toDate">The end date of the range.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing a list of schools or errors.</returns>
    public async Task<ErrorOr<List<School>>> GetSchoolsByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _repository.GetSchoolsByDateRangeAsync(fromDate, toDate);
    }

    /// <summary>
    /// Retrieves all versions of a school by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the school.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing a list of school versions or errors.</returns>
    public async Task<ErrorOr<List<School>>> GetAllVersionsOfSchoolAsync(int id)
    {
        return await _repository.GetAllVersionsAsync(id);
    }
}
