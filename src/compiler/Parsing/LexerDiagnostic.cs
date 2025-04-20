using Noa.Compiler.Diagnostics;

namespace Noa.Compiler.Parsing;

/// <summary>
/// A diagnostic internal to the lexer.
/// </summary>
internal interface ILexerDiagnostic
{
    IPartialDiagnostic ToPartial(int currentLexerPosition);
}

internal readonly record struct LexerDiagnostic<T>(
    DiagnosticTemplate<T> Template,
    T Arg,
    int Position,
    int Width)
    : ILexerDiagnostic
{
    public IPartialDiagnostic ToPartial(int currentLexerPosition) =>
        new PartialDiagnostic<T>(
            Template,
            Arg,
            Offset: Position - currentLexerPosition,
            Width);
}
