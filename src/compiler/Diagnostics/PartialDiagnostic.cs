using TextMappingUtils;

namespace Noa.Compiler.Diagnostics;

/// <summary>
/// A diagnostic with partial information about its location,
/// filled in after the fact once exact location information is known.
/// </summary>
public interface IPartialDiagnostic
{
    /// <summary>
    /// The template to create the diagnostic from.
    /// </summary>
    IDiagnosticTemplate Template { get; }

    /// <summary>
    /// The offset of the start of the diagnostic relative to the supplied location.
    /// </summary>
    int Offset { get; }

    /// <summary>
    /// The width of the diagnostic from its start.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Creates a full diagnostic by supplying a full position within the source to use as the location.
    /// </summary>
    /// <param name="source">The source the diagnostic is located in.</param>
    /// <param name="fullPosition">The full position in the source text which the diagnostic is relative to.</param>
    IDiagnostic Format(Source source, int fullPosition);

    /// <summary>
    /// Returns the partial diagnostic with an offset added.
    /// </summary>
    /// <param name="offset">The offset to add.</param>
    IPartialDiagnostic AddOffset(int offset);
}

/// <summary>
/// A diagnostic with partial information about its location,
/// filled in after the fact once exact location information is known.
/// </summary>
/// <param name="Template">The template to create the diagnostic from.</param>
/// <param name="Offset">The offset of the start of the diagnostic relative to the supplied location.</param>
/// <param name="Width">The width of the diagnostic from its start.</param>
public sealed record class PartialDiagnostic(
    DiagnosticTemplate Template,
    int Offset,
    int Width)
    : IPartialDiagnostic
{
    IDiagnosticTemplate IPartialDiagnostic.Template => Template;

    public IDiagnostic Format(Source source, int fullPosition)
    {
        var span = new TextSpan(
            start: fullPosition - Offset,
            end: fullPosition - Offset + Width);
        
        return Diagnostic.Create(
            Template,
            new Location(source.Name, span));
    }

    public IPartialDiagnostic AddOffset(int offset) =>
        new PartialDiagnostic(Template, Offset + offset, Width);
}

/// <summary>
/// A diagnostic with partial information about its location,
/// filled in after the fact once exact location information is known.
/// </summary>
/// <typeparam name="TArg">The type of the argument to the template.</typeparam>
/// <param name="Template">The template to create the diagnostic from.</param>
/// <param name="Argument">The argument to supply to the template.</param>
/// <param name="Offset">The offset of the start of the diagnostic relative to the supplied location.</param>
/// <param name="Width">The width of the diagnostic from its start.</param>
public sealed record class PartialDiagnostic<TArg>(
    DiagnosticTemplate<TArg> Template,
    TArg Argument,
    int Offset,
    int Width)
    : IPartialDiagnostic
{
    IDiagnosticTemplate IPartialDiagnostic.Template => Template;

    public IDiagnostic Format(Source source, int fullPosition)
    {
        var span = new TextSpan(
            start: fullPosition - Offset,
            end: fullPosition - Offset + Width);
        
        return Diagnostic.Create(
            Template,
            Argument,
            new Location(source.Name, span));
    }

    public IPartialDiagnostic AddOffset(int offset) =>
        new PartialDiagnostic<TArg>(Template, Argument, Offset + offset, Width);
}
