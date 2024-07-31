using Serilog.Events;
using Serilog.Formatting;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;

namespace DomainDrivenWebApplication.Infrastructure.Logging;

public class CustomSerilogJsonFormatter : ITextFormatter
{
    private readonly bool _useRelaxedEscaping;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    // Regular expressions for redacting sensitive information
    private static readonly Regex EmailRegex = new Regex(@"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b", RegexOptions.IgnoreCase);
    private static readonly Regex PhoneRegex = new Regex(@"\b\d{3}[-.]?\d{3}[-.]?\d{4}\b", RegexOptions.IgnoreCase);

    public CustomSerilogJsonFormatter(bool useRelaxedEscaping)
    {
        _useRelaxedEscaping = useRelaxedEscaping;
    }

    public void Format(LogEvent logEvent, TextWriter output)
    {
        var json = new
        {
            Timestamp = logEvent.Timestamp.ToString("o"),
            Level = logEvent.Level.ToString(),
            Message = RedactSensitiveInformation(logEvent.RenderMessage()),
            Exception = logEvent.Exception?.ToString(),
            CorrelationId = logEvent.Properties.ContainsKey(CorrelationIdHeader)
                ? RedactSensitiveInformation(logEvent.Properties[CorrelationIdHeader].ToString())
                : null,
            Properties = logEvent.Properties
                .Where(p => p.Key != CorrelationIdHeader)
                .Select(p => new
                {
                    Key = p.Key,
                    Value = RedactSensitiveInformation(FormatPropertyValue(p.Value))
                })
        };

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = _useRelaxedEscaping
                ? System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                : System.Text.Encodings.Web.JavaScriptEncoder.Default,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        string jsonString = JsonSerializer.Serialize(json, options);
        output.WriteLine(jsonString);
    }

    private static string RedactSensitiveInformation(string message)
    {
        if (string.IsNullOrEmpty(message))
            return message;

        // Redact email addresses
        message = EmailRegex.Replace(message, "REDACTED_EMAIL");
        // Redact phone numbers
        message = PhoneRegex.Replace(message, "REDACTED_PHONE");

        return message;
    }

    private static string FormatPropertyValue(LogEventPropertyValue propertyValue)
    {
        switch (propertyValue)
        {
            case ScalarValue scalarValue:
                return RedactSensitiveInformation(scalarValue.ToString());
            case SequenceValue sequenceValue:
                return $"[ {string.Join(", ", sequenceValue.Elements.Select(FormatPropertyValue))} ]";
            case StructureValue structureValue:
                return $"{{ {string.Join(", ", structureValue.Properties.Select(p => $"{p.Name}: {FormatPropertyValue(p.Value)}"))} }}";
            case DictionaryValue dictionaryValue:
                return $"{{ {string.Join(", ", dictionaryValue.Elements.Select(e => $"{FormatPropertyValue(e.Key)}: {FormatPropertyValue(e.Value)}"))} }}";
            default:
                return propertyValue.ToString();
        }
    }
}