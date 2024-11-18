using DomainDrivenWebApplication.Domain.Entities;
using ErrorOr;

namespace DomainDrivenWebApplication.Domain.Interfaces;

/// <summary>
/// Interface for school repository to manage school entities and their operations.
/// </summary>
public interface ISchoolRepository
{
    /// <summary>
    /// Retrieves a school entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the school.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> result containing the school entity if found, or an error.</returns>
    Task<ErrorOr<School>> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves all school entities.
    /// </summary>
    /// <returns>An <see cref="ErrorOr{T}"/> result containing a list of all school entities, or an error.</returns>
    Task<ErrorOr<List<School>>> GetAllAsync();

    /// <summary>
    /// Adds a new school entity.
    /// </summary>
    /// <param name="school">The school entity to add.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> result indicating success or failure.</returns>
    Task<ErrorOr<bool>> AddAsync(School school);

    /// <summary>
    /// Updates an existing school entity.
    /// </summary>
    /// <param name="school">The school entity to update.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> result indicating success or failure.</returns>
    Task<ErrorOr<bool>> UpdateAsync(School school);

    /// <summary>
    /// Deletes an existing school entity.
    /// </summary>
    /// <param name="school">The school entity to delete.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> result indicating success or failure.</returns>
    Task<ErrorOr<bool>> DeleteAsync(School school);

    /// <summary>
    /// Retrieves all school entities within a specific date range.
    /// </summary>
    /// <param name="fromDate">The start date of the range.</param>
    /// <param name="toDate">The end date of the range.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> result containing a list of schools within the date range, or an error.</returns>
    Task<ErrorOr<List<School>>> GetSchoolsByDateRangeAsync(DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Retrieves all versions of a specific school entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the school.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> result containing a list of all versions of the school, or an error.</returns>
    Task<ErrorOr<List<School>>> GetAllVersionsAsync(int id);
}