using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;

namespace DomainDrivenWebApplication.API.Controllers;

/// <summary>
/// A base controller providing common functionality for handling errors in API responses.
/// </summary>
public class BaseController : ControllerBase
{
    protected readonly ILogger<BaseController> _logger;
    protected readonly IStringLocalizer<BaseController> _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="localizer">The string localizer instance.</param>
    public BaseController(ILogger<BaseController> logger, IStringLocalizer<BaseController> localizer)
    {
        _logger = logger;
        _localizer = localizer;
    }

    /// <summary>
    /// Handles a collection of errors and returns an appropriate HTTP response.
    /// </summary>
    /// <param name="errors">A collection of errors to process.</param>
    /// <returns>An <see cref="IActionResult"/> representing the HTTP response.</returns>
    [NonAction]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult HandleErrors(IEnumerable<Error> errors)
    {
        if (errors == null || !errors.Any())
        {
            _logger.LogError("An unknown error occurred with no details provided.");
            return Problem();
        }

        _logger.LogError("Errors encountered: {Errors}", errors);

        if (errors.All(error => error.Type == ErrorType.Validation))
        {
            return GenerateValidationProblem(errors);
        }

        return GenerateErrorResponse(errors.ToList());
    }

    /// <summary>
    /// Generates an error response based on the provided list of errors.
    /// Sets error details in the HTTP context and returns the appropriate ProblemDetails response.
    /// </summary>
    /// <param name="errors">A list of <see cref="Error"/> objects representing the errors that need to be processed.</param>
    /// <returns>A configured <see cref="ObjectResult"/> containing the problem details for the error response.</returns>
    private ObjectResult GenerateErrorResponse(List<Error> errors)
    {
        if (HttpContext != null)
        {
            HttpContext.Items["errorDetails"] = new ProblemDetails
            {
                Status = GetStatusCodeForError(errors.First()),
                Title = errors.First().Code,
                Detail = errors.First().Description,
            };
        }

        Error primaryError = errors.First();
        int statusCode = GetStatusCodeForErrorType(primaryError.Type);
        LocalizedString localizedDetail = _localizer[primaryError.Code];

        return Problem(
            statusCode: statusCode,
            title: primaryError.Code,
            detail: localizedDetail
        );
    }

    /// <summary>
    /// Gets the corresponding HTTP status code for a given <see cref="ErrorType"/>.
    /// </summary>
    /// <param name="errorType">The <see cref="ErrorType"/> representing the type of error.</param>
    /// <returns>An integer representing the HTTP status code.</returns>
    private int GetStatusCodeForErrorType(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status403Forbidden,
            ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
            ErrorType.Failure => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
    }



    /// <summary>
    /// Creates an HTTP validation problem response for validation errors.
    /// </summary>
    /// <param name="validationErrors">A collection of validation errors to process.</param>
    /// <returns>An <see cref="ActionResult"/> representing the HTTP response.</returns>
    private ActionResult GenerateValidationProblem(IEnumerable<Error> validationErrors)
    {
        ModelStateDictionary modelStateDictionary = new ModelStateDictionary();

        foreach (Error validationError in validationErrors)
        {
            modelStateDictionary.AddModelError(validationError.Code, _localizer[validationError.Code]);
        }

        return ValidationProblem(modelStateDictionary);
    }

    /// <summary>
    /// Gets the HTTP status code for a specific error type.
    /// </summary>
    /// <param name="error">The error for which to get the status code.</param>
    /// <returns>An HTTP status code corresponding to the error type.</returns>
    private static int GetStatusCodeForError(Error error) =>
        error.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status403Forbidden,
            ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
            ErrorType.Failure => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError,
        };
}
