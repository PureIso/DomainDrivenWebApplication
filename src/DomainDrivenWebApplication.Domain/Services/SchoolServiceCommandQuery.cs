using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Domain.Interfaces;
using ErrorOr;

namespace DomainDrivenWebApplication.Domain.Services;

/// <summary>
/// Service class responsible for business logic operations related to schools.
/// Separates query and command responsibilities through repositories.
/// </summary>
public class SchoolServiceCommandQuery
{
    private readonly ISchoolCommandRepository _commandRepository;
    private readonly ISchoolQueryRepository _queryRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchoolServiceCommandQuery"/> class.
    /// </summary>
    /// <param name="commandRepository">The repository for managing write operations.</param>
    /// <param name="queryRepository">The repository for managing read operations.</param>
    public SchoolServiceCommandQuery(
        ISchoolCommandRepository commandRepository,
        ISchoolQueryRepository queryRepository)
    {
        _commandRepository = commandRepository;
        _queryRepository = queryRepository;
    }

    /// <summary>
    /// Retrieves a school by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The identifier of the school to retrieve.</param>
    /// <returns>The school entity wrapped in an <see cref="ErrorOr{T}"/>, or an error if not found.</returns>
    public async Task<ErrorOr<School>> GetSchoolByIdAsync(int id)
    {
        return await _queryRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Retrieves all schools asynchronously.
    /// </summary>
    /// <returns>An <see cref="ErrorOr{T}"/> containing a list of school entities, or an error if retrieval fails.</returns>
    public async Task<ErrorOr<List<School>>> GetAllSchoolsAsync()
    {
        return await _queryRepository.GetAllAsync();
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

        school.PrincipalName = string.IsNullOrWhiteSpace(school.PrincipalName)
            ? "Default Principal"
            : school.PrincipalName;

        return await _commandRepository.AddAsync(school);
    }

    /// <summary>
    /// Updates an existing school asynchronously.
    /// </summary>
    /// <param name="school">The school entity to update.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> indicating success or failure.</returns>
    public async Task<ErrorOr<bool>> UpdateSchoolAsync(School school)
    {
        return await _commandRepository.UpdateAsync(school);
    }

    /// <summary>
    /// Deletes a school by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The identifier of the school to delete.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> indicating success or failure.</returns>
    public async Task<ErrorOr<bool>> DeleteSchoolAsync(int id)
    {
        ErrorOr<School> schoolResult = await _queryRepository.GetByIdAsync(id);
        if (schoolResult.IsError)
        {
            return schoolResult.Errors;
        }

        return await _commandRepository.DeleteAsync(schoolResult.Value);
    }

    /// <summary>
    /// Retrieves schools within a specified date range asynchronously.
    /// </summary>
    /// <param name="fromDate">The start date of the range.</param>
    /// <param name="toDate">The end date of the range.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing a list of schools within the specified date range, or an error.</returns>
    public async Task<ErrorOr<List<School>>> GetSchoolsByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _queryRepository.GetSchoolsByDateRangeAsync(fromDate, toDate);
    }

    /// <summary>
    /// Retrieves all versions of a school asynchronously.
    /// </summary>
    /// <param name="id">The identifier of the school.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing a list of all versions of the school entity, or an error.</returns>
    public async Task<ErrorOr<List<School>>> GetAllVersionsOfSchoolAsync(int id)
    {
        return await _queryRepository.GetAllVersionsAsync(id);
    }
}
