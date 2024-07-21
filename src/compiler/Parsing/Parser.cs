using Noa.Compiler.Diagnostics;
using Noa.Compiler.Nodes;
using Expression = Noa.Compiler.Nodes.Expression;

namespace Noa.Compiler.Parsing;

/// <summary>
/// Parses source files into AST nodes.
/// </summary>
internal sealed partial class Parser
{
    /// <summary>
    /// Parses a source file into a root node.
    /// </summary>
    /// <param name="source">The source file to parse.</param>
    /// <param name="ast">The AST which parsed nodes belong to.</param>
    /// <param name="cancellationToken">The cancellation token used to signal the parser to cancel.</param>
    public static (Root, IReadOnlyCollection<IDiagnostic>) Parse(
        Source source,
        Ast ast,
        CancellationToken cancellationToken)
    {
        var (tokens, lexDiagnostics) = Lexer.Lex(source, cancellationToken);
        var parser = new Parser(source, ast, tokens, cancellationToken);
        
        var root = parser.ParseRoot();
        var diagnostics = parser.Diagnostics;
        
        return (root, lexDiagnostics.Concat(diagnostics).ToList());
    }

    internal Root ParseRoot()
    {
        var (statements, trailingExpression) = ParseBlock(
            allowTrailingExpression: true,
            endKind: TokenKind.EndOfFile,
            synchronizationTokens: SyntaxFacts.RootSynchronize);

        var endOfFile = Expect(TokenKind.EndOfFile);

        var start = statements.FirstOrDefault()?.Span.Start
                    ?? trailingExpression?.Span.Start
                    ?? endOfFile.Span.Start;

        return new()
        {
            Ast = Ast,
            Span = endOfFile.Span with { Start = start },
            Statements = statements,
            TrailingExpression = trailingExpression
        };
    }

    internal Identifier ParseIdentifier()
    {
        var name = Expect(TokenKind.Name);

        return new()
        {
            Ast = Ast,
            Span = name.Span,
            Name = name.Text
        };
    }

    internal Declaration ParseDeclaration() => Current.Kind switch
    {
        TokenKind.Func => ParseFunctionDeclaration(),
        TokenKind.Let => ParseLetDeclaration(),
        _ => throw new InvalidOperationException(
            $"Cannot parse a declaration starting with {Current.Kind}.")
    };

    internal FunctionDeclaration ParseFunctionDeclaration()
    {
        var func = Expect(TokenKind.Func);

        var identifier = ParseIdentifier();

        Expect(TokenKind.OpenParen);

        var parameters = ParseSeparatedList(
            TokenKind.Comma,
            true,
            ParseParameter,
            TokenKind.CloseParen,
            TokenKind.EqualsGreaterThan,
            TokenKind.OpenBrace);

        Expect(TokenKind.CloseParen);

        var expressionBody = null as (Expression expression, Token semicolon)?;
        var blockBody = null as BlockExpression;
        
        if (Current.Kind is TokenKind.OpenBrace)
        {
            blockBody = ParseBlockExpression();
        }
        else
        {
            Expect(TokenKind.EqualsGreaterThan);

            var expression = ParseExpressionOrError();
            
            var semicolon = Expect(TokenKind.Semicolon);

            expressionBody = (expression, semicolon);
        }

        var end = (expressionBody, blockBody) switch
        {
            (var (_, semicolon), null) => semicolon.Span.End,
            (null, not null) => blockBody.Span.End,
            // It's impossible for the expression body and block body to both be null or both not be null.
            _ => throw new UnreachableException()
        };

        return new()
        {
            Ast = Ast,
            Span = func.Span with { End = end },
            FuncKeyword = func,
            Identifier = identifier,
            Parameters = parameters,
            ExpressionBody = expressionBody?.expression,
            BlockBody = blockBody
        };
    }

    internal Parameter ParseParameter()
    {
        var mutToken = Current.Kind is TokenKind.Mut
            ? Advance()
            : null as Token?;

        var identifier = ParseIdentifier();

        var start = mutToken?.Span.Start ?? identifier.Span.Start;
        
        return new()
        {
            Ast = Ast,
            Span = identifier.Span with { Start = start },
            IsMutable = mutToken is not null,
            Identifier = identifier
        };
    }

    internal LetDeclaration ParseLetDeclaration()
    {
        var let = Expect(TokenKind.Let);

        bool isMutable;
        if (Current.Kind is TokenKind.Mut)
        {
            isMutable = true;
            Advance();
        }
        else isMutable = false;

        var identifier = ParseIdentifier();

        Expect(TokenKind.Equals);

        var expression = ParseExpressionOrError();

        var semicolon = Expect(TokenKind.Semicolon);

        return new()
        {
            Ast = Ast,
            Span = TextSpan.Between(let.Span, semicolon.Span),
            LetKeyword = let,
            IsMutable = isMutable,
            Identifier = identifier,
            Expression = expression
        };
    }
}
