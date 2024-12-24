using DomainDrivenWebApplication.Domain.Entities;
using ErrorOr;

namespace DomainDrivenWebApplication.Domain.Interfaces;

/// <summary>
/// Interface for the query repository managing read operations for schools.
/// </summary>
public interface ISchoolQueryRepository
{
    /// <summary>
    /// Gets a school by its unique identifier (ID).
    /// </summary>
    /// <param name="id">The unique identifier of the school.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing the school if found, otherwise a "not found" error.</returns>
    Task<ErrorOr<School>> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves all schools from the database.
    /// </summary>
    /// <returns>An <see cref="ErrorOr{T}"/> containing a list of schools if any are found, otherwise a "not found" error.</returns>
    Task<ErrorOr<List<School>>> GetAllAsync();

    /// <summary>
    /// Retrieves schools within a specified date range based on their temporal validity.
    /// </summary>
    /// <param name="fromDate">The start date of the range.</param>
    /// <param name="toDate">The end date of the range.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing a list of schools that are valid within the date range, or a "not found" error if no schools are found.</returns>
    Task<ErrorOr<List<School>>> GetSchoolsByDateRangeAsync(DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Retrieves all versions of a school based on its unique identifier (ID).
    /// </summary>
    /// <param name="id">The unique identifier of the school.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> containing a list of all versions of the specified school, or a "not found" error if no versions are found.</returns>
    Task<ErrorOr<List<School>>> GetAllVersionsAsync(int id);
}