using Noa.Compiler.Syntax.Green;

namespace Noa.Compiler.Parsing;

internal readonly record struct TriviaToken(Token Token, TriviaTokenKind Kind)
{
    public Token AttachTo(Token token) => Kind switch
    {
        TriviaTokenKind.Unexpected => token.AddTokenAsUnexpectedTrivia(Token),
        TriviaTokenKind.Skipped => token.AddTokenAsSkippedTrivia(Token),
        _ => throw new UnreachableException()
    };
}

internal enum TriviaTokenKind
{
    Unexpected,
    Skipped
}
