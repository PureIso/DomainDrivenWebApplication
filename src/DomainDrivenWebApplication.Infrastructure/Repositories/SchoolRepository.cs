using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Domain.Interfaces;
using DomainDrivenWebApplication.Infrastructure.Data;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace DomainDrivenWebApplication.Infrastructure.Repositories;

/// <summary>
/// Repository for interacting with school data in the database.
/// Provides methods for CRUD operations and additional queries like retrieving schools by date range
/// and handling temporal data.
/// </summary>
public class SchoolRepository : ISchoolRepository
{
    private readonly SchoolContext _context;
    private readonly IStringLocalizer<SchoolRepository> _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchoolRepository"/> class.
    /// </summary>
    /// <param name="context">The <see cref="SchoolContext"/> used for database operations.</param>
    /// <param name="localizer">The <see cref="IStringLocalizer"/> used for localization of error messages.</param>
    public SchoolRepository(SchoolContext context, IStringLocalizer<SchoolRepository> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    /// <summary>
    /// Gets a school by its unique identifier (ID).
    /// </summary>
    /// <param name="id">The unique identifier of the school.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> object containing the school if found, otherwise a "not found" error.</returns>
    public async Task<ErrorOr<School>> GetByIdAsync(int id)
    {
        School? school = await _context.Schools.FindAsync(id);
        if (school == null)
        {
            string errorCode = "SchoolNotFound";
            return Error.NotFound(_localizer[errorCode], errorCode);
        }
        return school;
    }

    /// <summary>
    /// Retrieves all schools from the database.
    /// </summary>
    /// <returns>An <see cref="ErrorOr{T}"/> object containing a list of schools if any are found, otherwise a "not found" error.</returns>
    public async Task<ErrorOr<List<School>>> GetAllAsync()
    {
        List<School> schools = await _context.Schools.ToListAsync();
        if (schools.Any())
        {
            return schools;
        }

        string errorCode = "NoSchoolsFound";
        return Error.NotFound(_localizer[errorCode], errorCode);
    }

    /// <summary>
    /// Adds a new school to the database.
    /// </summary>
    /// <param name="school">The school to add.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> object indicating success (true) or failure with an error message.</returns>
    public async Task<ErrorOr<bool>> AddAsync(School school)
    {
        await _context.Schools.AddAsync(school);
        int entries = await _context.SaveChangesAsync();
        if (entries > 0)
        {
            return true;
        }

        string errorCode = "FailedToAddSchool";
        return Error.Failure(_localizer[errorCode], errorCode);
    }

    /// <summary>
    /// Updates an existing school in the database.
    /// </summary>
    /// <param name="school">The school with updated information.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> object indicating success (true) or failure with an error message.</returns>
    public async Task<ErrorOr<bool>> UpdateAsync(School school)
    {
        _context.Entry(school).State = EntityState.Modified;
        int entries = await _context.SaveChangesAsync();
        if (entries > 0)
        {
            return true;
        }

        string errorCode = "FailedToUpdateSchool";
        return Error.Failure(_localizer[errorCode], errorCode);
    }

    /// <summary>
    /// Deletes a school from the database.
    /// </summary>
    /// <param name="school">The school to delete.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> object indicating success (true) or failure with an error message.</returns>
    public async Task<ErrorOr<bool>> DeleteAsync(School school)
    {
        _context.Schools.Remove(school);
        int entries = await _context.SaveChangesAsync();
        if (entries > 0)
        {
            return true;
        }

        string errorCode = "FailedToDeleteSchool";
        return Error.Failure(_localizer[errorCode], errorCode);
    }

    /// <summary>
    /// Retrieves schools within a specified date range based on their temporal validity.
    /// </summary>
    /// <param name="fromDate">The start date of the range.</param>
    /// <param name="toDate">The end date of the range.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> object containing a list of schools that are valid within the date range, or a "not found" error if no schools are found.</returns>
    public async Task<ErrorOr<List<School>>> GetSchoolsByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        List<School> schools = await _context.Schools
            .TemporalAll()
            .Where(s =>
                EF.Property<DateTime>(s, "ValidFrom") <= toDate &&
                EF.Property<DateTime>(s, "ValidTo") >= fromDate)
            .OrderBy(s => EF.Property<DateTime>(s, "ValidFrom"))
            .ToListAsync();

        if (schools.Any())
        {
            return schools;
        }

        string errorCode = "NoSchoolsInDateRange";
        return Error.NotFound(_localizer[errorCode], errorCode);
    }

    /// <summary>
    /// Retrieves all versions of a school based on its unique identifier (ID).
    /// </summary>
    /// <param name="id">The unique identifier of the school.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> object containing a list of all versions of the specified school, or a "not found" error if no versions are found.</returns>
    public async Task<ErrorOr<List<School>>> GetAllVersionsAsync(int id)
    {
        List<School> schools = await _context.Schools
            .TemporalAll()
            .Where(s => s.Id == id)
            .OrderBy(s => EF.Property<DateTime>(s, "ValidFrom"))
            .ToListAsync();

        if (schools.Any())
        {
            return schools;
        }

        string errorCode = "NoSchoolVersionsFound";
        return Error.NotFound(_localizer[errorCode], errorCode);
    }
}