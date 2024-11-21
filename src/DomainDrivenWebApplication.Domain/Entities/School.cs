using System.ComponentModel.DataAnnotations;

namespace DomainDrivenWebApplication.Domain.Entities;

/// <summary>
/// School entity class representing a school with temporal tables in mind.
/// Contains sensitive data and metadata from the database.
/// </summary>
public class School
{
    /// <summary>
    /// Unique identifier for the school.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Name of the school.
    /// </summary>
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Address of the school.
    /// </summary>
    [Required, MaxLength(200)]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Principal's name of the school.
    /// </summary>
    [Required, MaxLength(50)]
    public string PrincipalName { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the school entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
