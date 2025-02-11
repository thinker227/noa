using Noa.Compiler.Diagnostics;
using Noa.Compiler.Syntax.Green;
using TextMappingUtils;
using TokenKind = Noa.Compiler.Syntax.TokenKind;

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
    public static RootSyntax Parse(
        Source source,
        Ast ast,
        CancellationToken cancellationToken)
    {
        var tokens = Lexer.Lex(source, cancellationToken);
        var parser = new Parser(source, ast, tokens, cancellationToken);
        
        var root = parser.ParseRoot();
        
        return root;
    }

    internal RootSyntax ParseRoot()
    {
        var (statements, trailingExpression) = ParseBlock(
            allowTrailingExpression: true,
            endKind: TokenKind.EndOfFile,
            synchronizationTokens: SyntaxFacts.RootSynchronize);

        var endOfFile = Expect(TokenKind.EndOfFile);

        return new()
        {
            Statements = statements,
            TrailingExpression = trailingExpression,
            EndOfFile = endOfFile
        };
    }

    internal DeclarationSyntax ParseDeclaration() => Current.Kind switch
    {
        TokenKind.Func => ParseFunctionDeclaration(),
        TokenKind.Let => ParseLetDeclaration(),
        _ => throw new InvalidOperationException(
            $"Cannot parse a declaration starting with {Current.Kind}.")
    };

    internal FunctionDeclarationSyntax ParseFunctionDeclaration()
    {
        var func = Expect(TokenKind.Func);

        var identifier = Expect(TokenKind.Name);

        var parameters = ParseParameterList();

        FunctionBodySyntax body;
        
        if (Current.Kind is TokenKind.OpenBrace)
        {
            var blockBody = ParseBlockExpression();
            body = new BlockBodySyntax()
            {
                Block = blockBody
            };
        }
        else
        {
            var arrow = Expect(TokenKind.EqualsGreaterThan);

            var expression = ParseExpressionOrError();
            
            var semicolon = Expect(TokenKind.Semicolon);

            body = new ExpressionBodySyntax()
            {
                Arrow = arrow,
                Expression = expression,
                Semicolon = semicolon
            };
        }

        return new()
        {
            Func = func,
            Name = identifier,
            Parameters = parameters,
            Body = body
        };
    }

    internal ParameterListSyntax ParseParameterList()
    {
        var openParen = Expect(TokenKind.OpenParen);

        var parameters = ParseSeparatedList(
            TokenKind.Comma,
            true,
            ParseParameter,
            TokenKind.CloseParen,
            TokenKind.EqualsGreaterThan,
            TokenKind.OpenBrace);

        var closeParen = Expect(TokenKind.CloseParen);

        return new()
        {
            OpenParen = openParen,
            Parameters = parameters,
            CloseParen = closeParen
        };
    }

    internal ParameterSyntax ParseParameter()
    {
        var mutToken = Current.Kind is TokenKind.Mut
            ? Advance()
            : null;

        var identifier = Expect(TokenKind.Name);
        
        return new()
        {
            Mut = mutToken,
            Name = identifier
        };
    }

    internal LetDeclarationSyntax ParseLetDeclaration()
    {
        var let = Expect(TokenKind.Let);

        var mut = Current.Kind is TokenKind.Mut
            ? Advance()
            : null;

        var identifier = Expect(TokenKind.Name);

        var equals = Expect(TokenKind.Equals);

        var expression = ParseExpressionOrError();

        var semicolon = Expect(TokenKind.Semicolon);

        return new()
        {
            Let = let,
            Mut = mut,
            Name = identifier,
            Equals = equals,
            Value = expression,
            Semicolon = semicolon
        };
    }
}
