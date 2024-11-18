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
    public int Id { get; set; }

    /// <summary>
    /// Name of the school.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Address of the school.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Principal's name of the school.
    /// Sensitive data (Example).
    /// </summary>
    public string PrincipalName { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the school entity was created.
    /// Metadata for the entity (data about the data).
    /// </summary>
    public DateTime CreatedAt { get; set; }
}