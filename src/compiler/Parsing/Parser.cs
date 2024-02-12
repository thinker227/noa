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
    public static (Root, IReadOnlyCollection<IDiagnostic>) Parse(Source source, Ast ast)
    {
        var tokens = Lexer.Lex(source);
        var parser = new Parser(source, ast, tokens);
        
        var root = parser.ParseRoot();
        var diagnostics = parser.Diagnostics;
        
        return (root, diagnostics);
    }

    internal Root ParseRoot()
    {
        var (statements, _) = ParseBlock(
            allowTrailingExpression: false,
            endKind: TokenKind.EndOfFile,
            synchronizationTokens: SyntaxFacts.RootSynchronize);

        var endOfFile = Expect(TokenKind.EndOfFile);

        var start = statements.FirstOrDefault()?.Location.Start ?? endOfFile.Location.Start;
        var location = new Location(Source.Name, start, endOfFile.Location.End);

        return new()
        {
            Ast = Ast,
            Location = location,
            Statements = statements
        };
    }

    internal Identifier ParseIdentifier()
    {
        var name = Expect(TokenKind.Name);

        return new()
        {
            Ast = Ast,
            Location = name.Location,
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
            (var (_, semicolon), null) => semicolon.Location.End,
            (null, not null) => blockBody.Location.End,
            // It's impossible for the expression body and block body to both be null or both not be null.
            _ => throw new UnreachableException()
        };

        return new()
        {
            Ast = Ast,
            Location = new(Source.Name, func.Location.Start, end),
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

        var start = mutToken?.Location.Start ?? identifier.Location.Start;
        
        return new()
        {
            Ast = Ast,
            Location = new(Source.Name, start, identifier.Location.End),
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
            Location = new(Source.Name, let.Location.Start, semicolon.Location.End),
            IsMutable = isMutable,
            Identifier = identifier,
            Expression = expression
        };
    }
}
