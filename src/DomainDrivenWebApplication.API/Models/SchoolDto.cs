namespace DomainDrivenWebApplication.API.Models;

/// <summary>
/// Safer representation of the School entity for the API.
/// </summary>
public class SchoolDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public DateTime CreatedAt { get; set; }
}