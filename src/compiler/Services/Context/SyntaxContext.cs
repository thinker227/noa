using Noa.Compiler.Syntax;

namespace Noa.Compiler.Services.Context;

/// <summary>
/// A context for a specific cursor position in a syntax tree.
/// </summary>
/// <param name="Kind">The kind of the context.</param>
/// <param name="Ast">The AST the context is within.</param>
/// <param name="LeftToken">The immediately token to the left of the cursor position.</param>
/// <param name="RightToken">The immediately token to the right of the cursor position.</param>
public sealed record SyntaxContext(
    int Position,
    SyntaxContextKind Kind,
    Ast Ast,
    Token? LeftToken,
    Token? RightToken);
