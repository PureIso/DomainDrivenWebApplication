using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Domain.Interfaces;
using DomainDrivenWebApplication.Infrastructure.Data;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace DomainDrivenWebApplication.Infrastructure.Repositories;

/// <summary>
/// Repository for managing school data with consistent CRUD and query operations.
/// </summary>
public class SchoolRepository : ISchoolRepository
{
    private readonly SchoolContext _context;
    private readonly IStringLocalizer<SchoolRepository> _localizer;

    private const string SchoolNotFoundErrorCode = "SchoolNotFound";
    private const string NoSchoolsFoundErrorCode = "NoSchoolsFound";
    private const string FailedToAddSchoolErrorCode = "FailedToAddSchool";
    private const string FailedToUpdateSchoolErrorCode = "FailedToUpdateSchool";
    private const string FailedToDeleteSchoolErrorCode = "FailedToDeleteSchool";
    private const string NoSchoolsInDateRangeErrorCode = "NoSchoolsInDateRange";
    private const string NoSchoolVersionsFoundErrorCode = "NoSchoolVersionsFound";

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

    /// <inheritdoc />
    public async Task<ErrorOr<School>> GetByIdAsync(int id)
    {
        School? school = await _context.Schools.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

        if (school == null)
        {
            return Error.NotFound(_localizer[SchoolNotFoundErrorCode], SchoolNotFoundErrorCode);
        }

        return school;
    }

    /// <inheritdoc />
    public async Task<ErrorOr<List<School>>> GetAllAsync()
    {
        List<School> schools = await _context.Schools.AsNoTracking().ToListAsync();

        if (!schools.Any())
        {
            return Error.NotFound(_localizer[NoSchoolsFoundErrorCode], NoSchoolsFoundErrorCode);
        }

        return schools;
    }

    /// <inheritdoc />
    public async Task<ErrorOr<bool>> AddAsync(School school)
    {
        try
        {
            await _context.Schools.AddAsync(school);
            int entries = await _context.SaveChangesAsync();

            if (entries <= 0)
            {
                return Error.Failure(_localizer[FailedToAddSchoolErrorCode], FailedToAddSchoolErrorCode);
            }

            _context.Entry(school).State = EntityState.Detached;
            return true;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message, _localizer[FailedToAddSchoolErrorCode]);
        }
    }

    /// <inheritdoc />
    public async Task<ErrorOr<bool>> UpdateAsync(School school)
    {
        try
        {
            School? existingSchool = await _context.Schools.FindAsync(school.Id);

            if (existingSchool == null)
            {
                return Error.NotFound(_localizer[SchoolNotFoundErrorCode], SchoolNotFoundErrorCode);
            }

            existingSchool.Name = school.Name;
            existingSchool.Address = school.Address;
            existingSchool.PrincipalName = school.PrincipalName;

            int entries = await _context.SaveChangesAsync();

            if (entries <= 0)
            {
                return Error.Failure(_localizer[FailedToUpdateSchoolErrorCode], FailedToUpdateSchoolErrorCode);
            }

            _context.Entry(existingSchool).State = EntityState.Detached;
            return true;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message, _localizer[FailedToUpdateSchoolErrorCode]);
        }
    }

    /// <inheritdoc />
    public async Task<ErrorOr<bool>> DeleteAsync(School school)
    {
        try
        {
            School? existingSchool = await _context.Schools.FindAsync(school.Id);

            if (existingSchool == null)
            {
                return Error.NotFound(_localizer[SchoolNotFoundErrorCode], SchoolNotFoundErrorCode);
            }

            _context.Schools.Remove(existingSchool);
            int entries = await _context.SaveChangesAsync();

            if (entries <= 0)
            {
                return Error.Failure(_localizer[FailedToDeleteSchoolErrorCode], FailedToDeleteSchoolErrorCode);
            }

            return true;
        }
        catch (Exception ex)
        {
            return Error.Failure(ex.Message, _localizer[FailedToDeleteSchoolErrorCode]);
        }
    }

    /// <inheritdoc />
    public async Task<ErrorOr<List<School>>> GetSchoolsByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        List<School> schools = await _context.Schools
            .TemporalAll()
            .AsNoTracking()
            .Where(s =>
                EF.Property<DateTime>(s, "ValidFrom") <= toDate &&
                EF.Property<DateTime>(s, "ValidTo") >= fromDate)
            .OrderBy(s => EF.Property<DateTime>(s, "ValidFrom"))
            .ToListAsync();

        if (!schools.Any())
        {
            return Error.NotFound(_localizer[NoSchoolsInDateRangeErrorCode], NoSchoolsInDateRangeErrorCode);
        }

        return schools;
    }

    /// <inheritdoc />
    public async Task<ErrorOr<List<School>>> GetAllVersionsAsync(int id)
    {
        List<School> schools = await _context.Schools
            .TemporalAll()
            .AsNoTracking()
            .Where(s => s.Id == id)
            .OrderBy(s => EF.Property<DateTime>(s, "ValidFrom"))
            .ToListAsync();

        if (!schools.Any())
        {
            return Error.NotFound(_localizer[NoSchoolVersionsFoundErrorCode], NoSchoolVersionsFoundErrorCode);
        }

        return schools;
    }
}
