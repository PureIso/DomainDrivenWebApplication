using ErrorOr;

namespace DomainDrivenWebApplication.Domain.Common.Models;

/// <summary>
/// Represents detailed information about an error.
/// </summary>
public class ErrorDetails
{
    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the list of errors.
    /// </summary>
    public List<Error> Errors { get; set; } = new();
}
