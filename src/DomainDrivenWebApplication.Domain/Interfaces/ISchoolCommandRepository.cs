using DomainDrivenWebApplication.Domain.Entities;
using ErrorOr;

namespace DomainDrivenWebApplication.Domain.Interfaces;

/// <summary>
/// Interface for the command repository managing write operations for schools.
/// </summary>
public interface ISchoolCommandRepository
{
    /// <summary>
    /// Adds a new school to the database.
    /// </summary>
    /// <param name="school">The school to add.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> indicating success (true) or failure with an error message.</returns>
    Task<ErrorOr<bool>> AddAsync(School school);

    /// <summary>
    /// Updates an existing school in the database.
    /// </summary>
    /// <param name="school">The school with updated information.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> indicating success (true) or failure with an error message.</returns>
    Task<ErrorOr<bool>> UpdateAsync(School school);

    /// <summary>
    /// Deletes a school from the database.
    /// </summary>
    /// <param name="school">The school to delete.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> indicating success (true) or failure with an error message.</returns>
    Task<ErrorOr<bool>> DeleteAsync(School school);
}