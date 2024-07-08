using System.Text.RegularExpressions;

namespace DomainDrivenWebApplication.API.Extensions;

/// <summary>
/// Custom Logger Provider for redacting sensitive information in log messages.
/// </summary>
public class CustomRedactingLoggerProvider : ILoggerProvider
{
    private readonly Action<LogLevel, EventId, object, Exception, Func<object, Exception, string>> _logAction;

    /// <summary>
    /// Constructor to initialize the logger action that performs redaction.
    /// </summary>
    public CustomRedactingLoggerProvider()
    {
        // Define the log action that redacts sensitive information
        _logAction = (logLevel, eventId, state, exception, formatter) =>
        {
            // Format the log message
            string message = formatter(state, exception);

            // Redact email addresses
            message = Regex.Replace(message, @"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b", "REDACTED_EMAIL", RegexOptions.IgnoreCase);

            // Redact phone numbers
            message = Regex.Replace(message, @"\b\d{3}[-.]?\d{3}[-.]?\d{4}\b", "REDACTED_PHONE", RegexOptions.IgnoreCase);

            // Print the log message to the console (for demonstration purposes)
            Console.WriteLine($"{logLevel}: {message}");
        };
    }

    /// <summary>
    /// Creates a new instance of the custom redacting logger.
    /// </summary>
    /// <param name="categoryName">The category name for the logger.</param>
    /// <returns>A new instance of CustomRedactingLogger.</returns>
    public ILogger CreateLogger(string categoryName)
    {
        return new CustomRedactingLogger(_logAction);
    }

    /// <summary>
    /// Disposes of any resources used by the logger provider.
    /// </summary>
    public void Dispose() { }
}

/// <summary>
/// Custom Logger that redacts sensitive information from log messages.
/// </summary>
public class CustomRedactingLogger : ILogger
{
    private readonly Action<LogLevel, EventId, object, Exception, Func<object, Exception, string>> _logAction;

    /// <summary>
    /// Constructor to initialize the logger with the redacting log action.
    /// </summary>
    /// <param name="logAction">The action to perform logging with redaction.</param>
    public CustomRedactingLogger(Action<LogLevel, EventId, object, Exception, Func<object, Exception, string>> logAction)
    {
        _logAction = logAction ?? throw new ArgumentNullException(nameof(logAction));
    }

    /// <summary>
    /// Begins a logical operation scope. (Not used in this custom logger.)
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="state">The state to set.</param>
    /// <returns>Returns null as this logger does not support scopes.</returns>
    public IDisposable BeginScope<TState>(TState state) => null;

    /// <summary>
    /// Checks if the specified log level is enabled. (Always returns true for this custom logger.)
    /// </summary>
    /// <param name="logLevel">The log level to check.</param>
    /// <returns>Always returns true.</returns>
    public bool IsEnabled(LogLevel logLevel) => true;

    /// <summary>
    /// Performs logging of a message with redaction of sensitive information.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="logLevel">The log level.</param>
    /// <param name="eventId">The event ID.</param>
    /// <param name="state">The state object.</param>
    /// <param name="exception">The exception object (if any).</param>
    /// <param name="formatter">The function to format the message.</param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        _logAction(logLevel, eventId, state, exception, (s, e) => formatter((TState)s, e));
    }
}