using Noa.Compiler.Symbols;
using Noa.Compiler.Syntax;
using SuperLinq;

namespace Noa.Compiler.Services.Context;

/// <summary>
/// A context for a specific cursor position in a syntax tree.
/// </summary>
/// <param name="Kind">The kind of the context.</param>
/// <param name="Ast">The AST the context is within.</param>
/// <param name="LeftToken">The immediately token to the left of the cursor position.</param>
/// <param name="RightToken">The immediately token to the right of the cursor position.</param>
/// <param name="AccessibleSymbols">The symbols which are accessible at the cursor position.</param>
public sealed record SyntaxContext(
    int Position,
    SyntaxContextKind Kind,
    Ast Ast,
    ITokenLike? LeftToken,
    ITokenLike? RightToken,
    IBuffer<ISymbol> AccessibleSymbols);
