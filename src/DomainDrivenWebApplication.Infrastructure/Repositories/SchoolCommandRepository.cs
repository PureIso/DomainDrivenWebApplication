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

            if (entries > 0)
            {
                return true;
            }

            const string errorCode = "FailedToAddSchool";
            return Error.Failure(_localizer[errorCode], errorCode);
        }
        catch (Exception ex)
        {
            return Error.Failure(_localizer["UnexpectedError"], ex.Message);
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
                string errorCode = "SchoolNotFound";
                return Error.NotFound(_localizer[errorCode], errorCode);
            }

            existingSchool.Name = school.Name;
            existingSchool.Address = school.Address;
            existingSchool.PrincipalName = school.PrincipalName;

            int entries = await context.SaveChangesAsync();
            context.Entry(existingSchool).State = EntityState.Detached;

            return entries > 0 ? true : Error.Failure(_localizer["FailedToUpdateSchool"], "FailedToUpdateSchool");
        }
        catch (Exception ex)
        {
            return Error.Failure(_localizer["UnexpectedError"], ex.Message);
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
                string errorCode = "SchoolNotFound";
                return Error.NotFound(_localizer[errorCode], errorCode);
            }

            context.Schools.Remove(existingSchool);
            int entries = await context.SaveChangesAsync();

            return entries > 0 ? true : Error.Failure(_localizer["FailedToDeleteSchool"], "FailedToDeleteSchool");
        }
        catch (Exception ex)
        {
            return Error.Failure(_localizer["UnexpectedError"], ex.Message);
        }
    }
}
