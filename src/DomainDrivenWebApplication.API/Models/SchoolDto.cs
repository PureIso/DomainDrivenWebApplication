namespace DomainDrivenWebApplication.API.Models;

/// <summary>
/// Safer representation of the School entity for the API.
/// </summary>
public class SchoolDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}