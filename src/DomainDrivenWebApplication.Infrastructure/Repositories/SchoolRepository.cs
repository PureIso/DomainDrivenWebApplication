using DomainDrivenWebApplication.Domain.Entities;
using DomainDrivenWebApplication.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using DomainDrivenWebApplication.Infrastructure.Data;

namespace DomainDrivenWebApplication.Infrastructure.Repositories;

/// <summary>
/// Repository for performing CRUD operations on School entities.
/// </summary>
public class SchoolRepository : ISchoolRepository
{
    private readonly SchoolContext _context;

    public SchoolRepository(SchoolContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a School entity by its ID asynchronously.
    /// </summary>
    /// <param name="id">The ID of the School to retrieve.</param>
    /// <returns>The School entity if found, otherwise null.</returns>
    public async Task<School?> GetByIdAsync(int id)
    {
        return await _context.Schools.FindAsync(id);
    }

    /// <summary>
    /// Retrieves all School entities asynchronously.
    /// </summary>
    /// <returns>A list of all School entities, or null if none exist.</returns>
    public async Task<List<School>?> GetAllAsync()
    {
        return await _context.Schools.ToListAsync();
    }

    /// <summary>
    /// Adds a new School entity asynchronously.
    /// </summary>
    /// <param name="school">The School entity to add.</param>
    /// <returns>True if changes to the database was made.</returns>
    public async Task<bool> AddAsync(School school)
    {
        await _context.Schools.AddAsync(school);
        int entries = await _context.SaveChangesAsync();
        return entries != 0;
    }

    /// <summary>
    /// Updates an existing School entity asynchronously.
    /// </summary>
    /// <param name="school">The School entity to update.</param>
    public async Task UpdateAsync(School school)
    {
        _context.Entry(school).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a School entity asynchronously.
    /// </summary>
    /// <param name="school">The School entity to delete.</param>
    public async Task DeleteAsync(School school)
    {
        _context.Schools.Remove(school);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves School entities that are valid within the specified date range.
    /// </summary>
    /// <param name="fromDate">Start date of the date range.</param>
    /// <param name="toDate">End date of the date range.</param>
    /// <returns>A list of School entities valid within the date range.</returns>
    public async Task<List<School>?> GetSchoolsByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _context.Schools
            .TemporalAll()
            .Where(s =>
                EF.Property<DateTime>(s, "ValidFrom") <= toDate &&
                EF.Property<DateTime>(s, "ValidTo") >= fromDate)
            .OrderBy(s => EF.Property<DateTime>(s, "ValidFrom"))
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves all versions (historical records) of a School entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the School entity.</param>
    /// <returns>A list of all versions of the School entity.</returns>
    public async Task<List<School>?> GetAllVersionsAsync(int id)
    {
        return await _context.Schools
            .TemporalAll()
            .Where(s => s.Id == id)
            .OrderBy(s => EF.Property<DateTime>(s, "ValidFrom"))
            .ToListAsync();
    }
}