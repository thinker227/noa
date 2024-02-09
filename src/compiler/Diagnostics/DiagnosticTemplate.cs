namespace Noa.Compiler.Diagnostics;

/// <summary>
/// A simple template for a diagnostic.
/// </summary>
/// <param name="Message">The message of the diagnostic.</param>
/// <param name="Severity">The severity of the diagnostic.</param>
public sealed record DiagnosticTemplate(DiagnosticId Id, string Message, Severity Severity)
{
    /// <summary>
    /// Formats the template into a diagnostic.
    /// </summary>
    /// <param name="location">The location of the diagnostic.</param>
    public IDiagnostic Format(Location location) =>
        Diagnostic.Create(this, location);

    public override string ToString() => Id.ToString();
    
    /// <summary>
    /// Creates a simple diagnostic template.
    /// </summary>
    /// <param name="message">The message of the diagnostic.</param>
    /// <param name="severity">The severity of the diagnostic.</param>
    public static DiagnosticTemplate Create(DiagnosticId id, string message, Severity severity) =>
        new(id, message, severity);

    /// <summary>
    /// Creates a diagnostic template with an argument used to format its message.
    /// </summary>
    /// <param name="createMessage">A function to create the message for a diagnostic.</param>
    /// <param name="severity">The severity of the diagnostic.</param>
    /// <typeparam name="TArg">The type of the argument to the template.</typeparam>
    public static DiagnosticTemplate<TArg> Create<TArg>(
        DiagnosticId id,
        Func<TArg, string> createMessage,
        Severity severity) =>
        new(id, createMessage, severity);
}

/// <summary>
/// A template for a diagnostic with an argument used to format its message.
/// </summary>
/// <param name="CreateMessage">A function to create the message for a diagnostic.</param>
/// <param name="Severity">The severity of the diagnostic.</param>
/// <typeparam name="TArg">The type of the argument to the template.</typeparam>
public sealed record DiagnosticTemplate<TArg>(
    DiagnosticId Id,
    Func<TArg, string> CreateMessage,
    Severity Severity)
{
    /// <summary>
    /// Formats the template into a diagnostic.
    /// </summary>
    /// <param name="argument">The argument to supply to the template.</param>
    /// <param name="location">The location of the diagnostic.</param>
    public IDiagnostic Format(TArg argument, Location location) =>
        Diagnostic.Create(this, argument, location);

    public override string ToString() => Id.ToString();
}