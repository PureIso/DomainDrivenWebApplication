using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Domain.Interfaces;
using ErrorOr;

namespace DomainDrivenWebApplication.Domain.Services;

/// <summary>
/// Service class responsible for business logic operations related to schools.
/// Implements methods defined in <see cref="ISchoolRepository"/>.
/// </summary>
public class SchoolService
{
    private readonly ISchoolRepository _schoolRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchoolService"/> class.
    /// </summary>
    /// <param name="schoolRepository">The repository to interact with school data.</param>
    public SchoolService(ISchoolRepository schoolRepository)
    {
        _schoolRepository = schoolRepository;
    }

    /// <summary>
    /// Retrieves a school by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The identifier of the school to retrieve.</param>
    /// <returns>The school entity wrapped in an <see cref="ErrorOr{T}"/>, or an error if not found.</returns>
    public async Task<ErrorOr<School>> GetSchoolByIdAsync(int id)
    {
        return await _schoolRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Retrieves all schools asynchronously.
    /// </summary>
    /// <returns>An <see cref="ErrorOr{T}"/> containing a list of school entities, or an error if retrieval fails.</returns>
    public async Task<ErrorOr<List<School>>> GetAllSchoolsAsync()
    {
        return await _schoolRepository.GetAllAsync();
    }

    /// <summary>
    /// Adds a new school asynchronously.
    /// </summary>
    /// <param name="school">The school entity to add.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> indicating success or failure.</returns>
    public async Task<ErrorOr<bool>> AddSchoolAsync(School school)
    {
        // Perform any business logic validations here if needed
        school.CreatedAt = DateTime.UtcNow;

        // Since PrincipalName is required but not part of the DTO, set a default value.
        // Note: This approach should be validated based on domain requirements.
        school.PrincipalName = string.IsNullOrWhiteSpace(school.PrincipalName) ? "Default Principal" : school.PrincipalName;

        return await _schoolRepository.AddAsync(school);
    }

    /// <summary>
    /// Updates an existing school asynchronously.
    /// </summary>
    /// <param name="school">The school entity to update.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> indicating success or failure.</returns>
    public async Task<ErrorOr<bool>> UpdateSchoolAsync(School school)
    {
        // Perform any business logic validations here if needed
        return await _schoolRepository.UpdateAsync(school);
    }

    /// <summary>
    /// Deletes a school by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The identifier of the school to delete.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> indicating success or failure.</returns>
    public async Task<ErrorOr<bool>> DeleteSchoolAsync(int id)
    {
        ErrorOr<School> schoolResult = await _schoolRepository.GetByIdAsync(id);
        if (schoolResult.IsError)
        {
            return schoolResult.Errors;
        }

        return await _schoolRepository.DeleteAsync(schoolResult.Value);
    }

    /// <summary>
    /// Retrieves schools within a specified date range asynchronously.
    /// </summary>
    /// <param name="fromDate">The start date of the range.</param>
    /// <param name="toDate">The end date of the range.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing a list of schools within the specified date range, or an error.</returns>
    public async Task<ErrorOr<List<School>>> GetSchoolsByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _schoolRepository.GetSchoolsByDateRangeAsync(fromDate, toDate);
    }

    /// <summary>
    /// Retrieves all versions of a school asynchronously.
    /// </summary>
    /// <param name="id">The identifier of the school.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing a list of all versions of the school entity, or an error.</returns>
    public async Task<ErrorOr<List<School>>> GetAllVersionsOfSchoolAsync(int id)
    {
        return await _schoolRepository.GetAllVersionsAsync(id);
    }
}
