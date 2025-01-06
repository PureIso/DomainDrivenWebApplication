using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Domain.Interfaces;
using DomainDrivenWebApplication.Infrastructure.Data;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace DomainDrivenWebApplication.Infrastructure.Repositories;

/// <summary>
/// Command repository for managing write operations on school data.
/// </summary>
public class SchoolCommandRepository : ISchoolCommandRepository
{
    private readonly IDbContextFactory<SchoolCommandContext> _contextFactory;
    private readonly IStringLocalizer<SchoolCommandRepository> _localizer;
    private const string FailedToAddSchoolErrorCode = "FailedToAddSchool";
    private const string FailedToUpdateSchoolErrorCode = "FailedToUpdateSchool";
    private const string FailedToDeleteSchoolErrorCode = "FailedToDeleteSchool";
    private const string UnexpectedErrorCode = "UnexpectedError";
    private const string SchoolNotFoundErrorCode = "SchoolNotFound";

    /// <summary>
    /// Initializes a new instance of the <see cref="SchoolCommandRepository"/> class.
    /// </summary>
    /// <param name="contextFactory">The factory for creating <see cref="SchoolCommandContext"/> instances.</param>
    /// <param name="localizer">The <see cref="IStringLocalizer"/> for localization of error messages.</param>
    public SchoolCommandRepository(
        IDbContextFactory<SchoolCommandContext> contextFactory,
        IStringLocalizer<SchoolCommandRepository> localizer)
    {
        _contextFactory = contextFactory;
        _localizer = localizer;
    }

    /// <inheritdoc />
    public async Task<ErrorOr<bool>> AddAsync(School school)
    {
        try
        {
            await using SchoolCommandContext context = await _contextFactory.CreateDbContextAsync();
            await context.Schools.AddAsync(school);
            int entries = await context.SaveChangesAsync();
            context.Entry(school).State = EntityState.Detached;

            if (entries <= 0)
            {
                return Error.Failure(_localizer[FailedToAddSchoolErrorCode], FailedToAddSchoolErrorCode);
            }

            return true;
        }
        catch (Exception ex)
        {
            return Error.Failure(_localizer[UnexpectedErrorCode], ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<ErrorOr<bool>> UpdateAsync(School school)
    {
        try
        {
            await using SchoolCommandContext context = await _contextFactory.CreateDbContextAsync();
            School? existingSchool = await context.Schools.FindAsync(school.Id);

            if (existingSchool == null)
            {
                return Error.NotFound(_localizer[SchoolNotFoundErrorCode], SchoolNotFoundErrorCode);
            }

            existingSchool.Name = school.Name;
            existingSchool.Address = school.Address;
            existingSchool.PrincipalName = school.PrincipalName;

            int entries = await context.SaveChangesAsync();
            context.Entry(existingSchool).State = EntityState.Detached;

            if (entries <= 0)
            {
                return Error.Failure(_localizer[FailedToUpdateSchoolErrorCode], FailedToUpdateSchoolErrorCode);
            }

            return true;
        }
        catch (Exception ex)
        {
            return Error.Failure(_localizer[UnexpectedErrorCode], ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<ErrorOr<bool>> DeleteAsync(School school)
    {
        try
        {
            await using SchoolCommandContext context = await _contextFactory.CreateDbContextAsync();
            School? existingSchool = await context.Schools.FindAsync(school.Id);

            if (existingSchool == null)
            {
                return Error.NotFound(_localizer[SchoolNotFoundErrorCode], SchoolNotFoundErrorCode);
            }

            context.Schools.Remove(existingSchool);
            int entries = await context.SaveChangesAsync();

            if (entries <= 0)
            {
                return Error.Failure(_localizer[FailedToDeleteSchoolErrorCode], FailedToDeleteSchoolErrorCode);
            }

            return true;
        }
        catch (Exception ex)
        {
            return Error.Failure(_localizer[UnexpectedErrorCode], ex.Message);
        }
    }
}
