using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Domain.Interfaces;

namespace DomainDrivenWebApplication.Domain.Services;

/// <summary>
/// Service class responsible for business logic operations related to schools.
/// </summary>
public class SchoolService
{
    private readonly ISchoolRepository _schoolRepository;

    public SchoolService(ISchoolRepository schoolRepository)
    {
        _schoolRepository = schoolRepository;
    }

    /// <summary>
    /// Retrieves a school by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The identifier of the school to retrieve.</param>
    /// <returns>The school entity if found, otherwise null.</returns>
    public async Task<School?> GetSchoolByIdAsync(int id)
    {
        return await _schoolRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Retrieves all schools asynchronously.
    /// </summary>
    /// <returns>A list of school entities, or null if no schools exist.</returns>
    public async Task<List<School>?> GetAllSchoolsAsync()
    {
        return await _schoolRepository.GetAllAsync();
    }

    /// <summary>
    /// Adds a new school asynchronously.
    /// </summary>
    /// <param name="school">The school entity to add.</param>
    /// <returns>True if changes to the database was made.</returns>
    public async Task<bool> AddSchoolAsync(School school)
    {
        // Perform any business logic validations here if needed
        school.CreatedAt = DateTime.UtcNow;
        return await _schoolRepository.AddAsync(school);
    }

    /// <summary>
    /// Updates an existing school asynchronously.
    /// </summary>
    /// <param name="school">The school entity to update.</param>
    public async Task UpdateSchoolAsync(School school)
    {
        // Perform any business logic validations here if needed
        await _schoolRepository.UpdateAsync(school);
    }

    /// <summary>
    /// Deletes a school by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The identifier of the school to delete.</param>
    public async Task DeleteSchoolAsync(int id)
    {
        School? school = await _schoolRepository.GetByIdAsync(id);
        if (school != null)
        {
            await _schoolRepository.DeleteAsync(school);
        }
    }

    /// <summary>
    /// Retrieves schools within a specified date range asynchronously.
    /// </summary>
    /// <param name="fromDate">The start date of the range.</param>
    /// <param name="toDate">The end date of the range.</param>
    /// <returns>A list of school entities within the specified date range.</returns>
    public async Task<List<School>?> GetSchoolsByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _schoolRepository.GetSchoolsByDateRangeAsync(fromDate, toDate);
    }

    /// <summary>
    /// Retrieves all versions of a school asynchronously.
    /// </summary>
    /// <param name="id">The identifier of the school.</param>
    /// <returns>A list of all versions of the school entity.</returns>
    public async Task<List<School>?> GetAllVersionsOfSchoolAsync(int id)
    {
        return await _schoolRepository.GetAllVersionsAsync(id);
    }
}