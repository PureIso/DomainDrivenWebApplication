using DomainDrivenWebApplication.Domain.Entities;

namespace DomainDrivenWebApplication.Domain.Interfaces;

public interface ISchoolRepository
{
    Task<School?> GetByIdAsync(int id);
    Task<List<School>?> GetAllAsync();
    Task<bool> AddAsync(School school);
    Task UpdateAsync(School school);
    Task DeleteAsync(School school);
    Task<List<School>?> GetSchoolsByDateRangeAsync(DateTime fromDate, DateTime toDate);
    Task<List<School>?> GetAllVersionsAsync(int id);
}