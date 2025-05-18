namespace Noa.Compiler.Diagnostics;

/// <summary>
/// A common interface for templates for diagnostics.
/// </summary>
public interface IDiagnosticTemplate
{
    /// <summary>
    /// The ID of the diagnostic.
    /// </summary>
    DiagnosticId Id { get; }

    /// <summary>
    /// The severity of the diagnostic.
    /// </summary>
    Severity Severity { get; }
}

/// <summary>
/// Provides static factory methods for creating diagnostic templates.
/// </summary>
public static class DiagnosticTemplate
{
    /// <summary>
    /// Creates a simple diagnostic template.
    /// </summary>
    /// <param name="id">The ID of the diagnostic.</param>
    /// <param name="message">The message of the diagnostic.</param>
    /// <param name="severity">The severity of the diagnostic.</param>
    public static DiagnosticTemplate<Unit> Create(
        DiagnosticId id,
        string message,
        Severity severity) =>
        new(id, (_, page) => page.Raw(message), severity);

    /// <summary>
    /// Creates a simple diagnostic template.
    /// </summary>
    /// <param name="id">The ID of the diagnostic.</param>
    /// <param name="writeMessage">
    /// A function the diagnostic message onto an <see cref="IDiagnosticPage"/>.
    /// </param>
    /// <param name="severity">The severity of the diagnostic.</param>
    public static DiagnosticTemplate<Unit> Create(
        DiagnosticId id,
        Action<IDiagnosticPage> writeMessage,
        Severity severity) =>
        new(id, (_, page) => writeMessage(page), severity);

    /// <summary>
    /// Creates a diagnostic template with an argument used to format its message.
    /// </summary>
    /// <param name="writeMessage">
    /// A function to write the diagnostic message onto an <see cref="IDiagnosticPage"/>.
    /// </param>
    /// <param name="severity">The severity of the diagnostic.</param>
    /// <typeparam name="TArg">The type of the argument to the template.</typeparam>
    public static DiagnosticTemplate<TArg> Create<TArg>(
        DiagnosticId id,
        Action<TArg, IDiagnosticPage> writeMessage,
        Severity severity) =>
        new(id, writeMessage, severity);
    
    /// <summary>
    /// Formats the template into a diagnostic.
    /// </summary>
    /// <param name="template">The template to format.</param>
    /// <param name="location">The location of the diagnostic.</param>
    public static IDiagnostic Format(this DiagnosticTemplate<Unit> template, Location location) =>
        template.Format(new Unit(), location);
}

/// <summary>
/// A template for a diagnostic with an argument used to format its message.
/// </summary>
/// <param name="WriteMessage">
/// A function to write the diagnostic message onto an <see cref="IDiagnosticPage"/>.
/// </param>
/// <param name="Severity">The severity of the diagnostic.</param>
/// <typeparam name="TArg">The type of the argument to the template.</typeparam>
public sealed record DiagnosticTemplate<TArg>(
    DiagnosticId Id,
    Action<TArg, IDiagnosticPage> WriteMessage,
    Severity Severity)
    : IDiagnosticTemplate
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
