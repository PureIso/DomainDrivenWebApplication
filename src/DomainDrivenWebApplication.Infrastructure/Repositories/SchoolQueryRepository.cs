﻿using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Domain.Interfaces;
using DomainDrivenWebApplication.Infrastructure.Data;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace DomainDrivenWebApplication.Infrastructure.Repositories;

/// <summary>
/// Query repository for retrieving school data.
/// </summary>
public class SchoolQueryRepository : ISchoolQueryRepository
{
    private readonly IDbContextFactory<SchoolQueryContext> _contextFactory;
    private readonly IStringLocalizer<SchoolQueryRepository> _localizer;
    private const string SchoolNotFoundErrorCode = "SchoolNotFound";
    private const string NoSchoolsFoundErrorCode = "NoSchoolsFound";
    private const string NoSchoolsInDateRangeErrorCode = "NoSchoolsInDateRange";
    private const string NoSchoolVersionsFoundErrorCode = "NoSchoolVersionsFound";

    /// <summary>
    /// Initializes a new instance of the <see cref="SchoolQueryRepository"/> class.
    /// </summary>
    /// <param name="contextFactory">The <see cref="IDbContextFactory{QuerySchoolContext}"/> for creating DbContext instances.</param>
    /// <param name="localizer">The <see cref="IStringLocalizer"/> for localization of error messages.</param>
    public SchoolQueryRepository(IDbContextFactory<SchoolQueryContext> contextFactory, IStringLocalizer<SchoolQueryRepository> localizer)
    {
        _contextFactory = contextFactory;
        _localizer = localizer;
    }

    /// <inheritdoc />
    public async Task<ErrorOr<School>> GetByIdAsync(int id)
    {
        await using SchoolQueryContext context = await _contextFactory.CreateDbContextAsync();
        School? school = await context.Schools.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

        if (school == null)
        {
            return Error.NotFound(_localizer[SchoolNotFoundErrorCode], SchoolNotFoundErrorCode);
        }

        return school;
    }

    /// <inheritdoc />
    public async Task<ErrorOr<List<School>>> GetAllAsync()
    {
        await using SchoolQueryContext context = await _contextFactory.CreateDbContextAsync();
        List<School> schools = await context.Schools.AsNoTracking().ToListAsync();

        if (!schools.Any())
        {
            return Error.NotFound(_localizer[NoSchoolsFoundErrorCode], NoSchoolsFoundErrorCode);
        }

        return schools;
    }

    /// <inheritdoc />
    public async Task<ErrorOr<List<School>>> GetSchoolsByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        await using SchoolQueryContext context = await _contextFactory.CreateDbContextAsync();
        List<School> schools = await context.Schools
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
        await using SchoolQueryContext context = await _contextFactory.CreateDbContextAsync();
        List<School> schools = await context.Schools
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
