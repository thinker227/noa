namespace Noa.Compiler;

/// <summary>
/// A diagnostic produced by the compiler.
/// </summary>
/// <param name="Message">The message describing the diagnostic.</param>
/// <param name="Severity">The severity of the diagnostic.</param>
/// <param name="Location">The location in source of the diagnostic.</param>
public readonly record struct Diagnostic(string Message, Severity Severity, Location Location)
{
    public override string ToString()
    {
        var severity = Severity switch
        {
            Severity.Warning => "warning",
            Severity.Error => "error",
            _ => throw new UnreachableException()
        };

        return $"\"{Message}\" ({severity}) at {Location}";
    }
}

/// <summary>
/// The severity of a <see cref="Diagnostic"/>.
/// </summary>
public enum Severity
{
    Warning,
    Error,
}
