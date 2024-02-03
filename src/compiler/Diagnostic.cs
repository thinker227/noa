namespace Noa.Compiler;

/// <summary>
/// A diagnostic produced by the compiler.
/// </summary>
public interface IDiagnostic
{
    /// <summary>
    /// The message describing the diagnostic.
    /// </summary>
    string Message { get; }
    
    /// <summary>
    /// The severity of the diagnostic.
    /// </summary>
    Severity Severity { get; }
    
    /// <summary>
    /// The location in source of the diagnostic.
    /// </summary>
    Location Location { get; }
}

/// <summary>
/// The severity of a <see cref="Diagnostic"/>.
/// </summary>
public enum Severity
{
    Warning,
    Error,
}

/// <summary>
/// Static helpers for creating diagnostics. 
/// </summary>
public static class Diagnostic
{
    /// <summary>
    /// Creates a simple diagnostic from a diagnostic template and a location.
    /// </summary>
    /// <param name="template">The template to create the diagnostic from.</param>
    /// <param name="location">The location of the diagnostic.</param>
    public static IDiagnostic Create(DiagnosticTemplate template, Location location) =>
        new SimpleDiagnostic(template, location);

    /// <summary>
    /// Creates a diagnostic with from a template with an argument and a location.
    /// </summary>
    /// <param name="template">The template to create the diagnostic from.</param>
    /// <param name="argument">The argument to supply to the template.</param>
    /// <param name="location">The location of the diagnostic.</param>
    /// <typeparam name="TArg">The type of the argument to the template.</typeparam>
    public static IDiagnostic Create<TArg>(DiagnosticTemplate<TArg> template, TArg argument, Location location) =>
        new ArgumentDiagnostic<TArg>(template, argument, location);

    private static string DisplayDiagnostic(string message, Severity severity, Location location)
    {
         var severityString = severity switch
         {
             Severity.Warning => "warning",
             Severity.Error => "error",
             _ => throw new UnreachableException()
         };

         return $"\"{message}\" ({severityString}) at {location}";
    }
    
    private sealed class SimpleDiagnostic(DiagnosticTemplate template, Location location) : IDiagnostic
    {
        public string Message { get; } = template.Message;
    
        public Severity Severity { get; } = template.Severity;
    
        public Location Location { get; } = location;

        public override string ToString() => DisplayDiagnostic(Message, Severity, Location);
    }
    
    private sealed class ArgumentDiagnostic<TArg>(
        DiagnosticTemplate<TArg> template,
        TArg argument,
        Location location) : IDiagnostic
    {
        public string Message => template.CreateMessage(argument);

        public Severity Severity { get; } = template.Severity;

        public Location Location { get; } = location;

        public override string ToString() => DisplayDiagnostic(Message, Severity, Location);
    }
}

/// <summary>
/// A simple template for a diagnostic.
/// </summary>
/// <param name="Message">The message of the diagnostic.</param>
/// <param name="Severity">The severity of the diagnostic.</param>
public sealed record DiagnosticTemplate(string Message, Severity Severity)
{
    /// <summary>
    /// Formats the template into a diagnostic.
    /// </summary>
    /// <param name="location">The location of the diagnostic.</param>
    public IDiagnostic Format(Location location) =>
        Diagnostic.Create(this, location);
    
    /// <summary>
    /// Creates a simple diagnostic template.
    /// </summary>
    /// <param name="message">The message of the diagnostic.</param>
    /// <param name="severity">The severity of the diagnostic.</param>
    public static DiagnosticTemplate Create(string message, Severity severity) =>
        new(message, severity);

    /// <summary>
    /// Creates a diagnostic template with an argument used to format its message.
    /// </summary>
    /// <param name="createMessage">A function to create the message for a diagnostic.</param>
    /// <param name="severity">The severity of the diagnostic.</param>
    /// <typeparam name="TArg">The type of the argument to the template.</typeparam>
    public static DiagnosticTemplate<TArg> Create<TArg>(Func<TArg, string> createMessage, Severity severity) =>
        new(createMessage, severity);
}

/// <summary>
/// A template for a diagnostic with an argument used to format its message.
/// </summary>
/// <param name="CreateMessage">A function to create the message for a diagnostic.</param>
/// <param name="Severity">The severity of the diagnostic.</param>
/// <typeparam name="TArg">The type of the argument to the template.</typeparam>
public sealed record DiagnosticTemplate<TArg>(Func<TArg, string> CreateMessage, Severity Severity)
{
    /// <summary>
    /// Formats the template into a diagnostic.
    /// </summary>
    /// <param name="argument">The argument to supply to the template.</param>
    /// <param name="location">The location of the diagnostic.</param>
    public IDiagnostic Format(TArg argument, Location location) =>
        Diagnostic.Create(this, argument, location);
}
