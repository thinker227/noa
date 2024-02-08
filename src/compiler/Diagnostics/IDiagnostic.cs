namespace Noa.Compiler.Diagnostics;

/// <summary>
/// A diagnostic produced by the compiler.
/// </summary>
public interface IDiagnostic
{
    /// <summary>
    /// The ID of the diagnostic.
    /// </summary>
    DiagnosticId Id { get; }
    
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
