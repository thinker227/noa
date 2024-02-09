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
        var statements = ImmutableArray.CreateBuilder<Statement>();
        
        while (!AtEnd)
        {
            var statement = ParseStatementOrNull();

            if (statement is not null)
            {
                statements.Add(statement);
                continue;
            }
            
            // An unexpected token was encountered.
            var diagnostic = ParseDiagnostics.UnexpectedToken.Format(Current, Current.Location);
            ReportDiagnostic(diagnostic);
            
            // Try synchronize with the next statement.
            while (!AtEnd && !SyntaxFacts.RootSynchronize.Contains(Current.Kind)) Advance();
        }

        var endOfFile = Expect(TokenKind.EndOfFile);

        var start = statements.FirstOrDefault()?.Location.Start ?? endOfFile.Location.Start;
        var location = new Location(Source.Name, start, endOfFile.Location.End);

        return new()
        {
            Ast = Ast,
            Location = location,
            Statements = statements.ToImmutable()
        };
    }

    internal Statement? ParseStatementOrNull()
    {
        var declarationOrExpression = ParseDeclarationOrExpressionOrNull();

        if (declarationOrExpression is not var (declaration, expression)) return null;
        
        if (expression is not null && !expression.IsExpressionStatement())
        {
            // Only expression *statements* are allowed here.

            var diagnostic = ParseDiagnostics.InvalidExpressionStatement.Format(expression.Location);
            ReportDiagnostic(diagnostic);
        }

        var semicolon = Expect(TokenKind.Semicolon);

        var start = (declaration, expression) switch
        {
            (not null, null) => declaration.Location.Start,
            (null, not null) => expression.Location.Start,
            // It's impossible for both the declaration and expression to not be null.
            _ => throw new UnreachableException()
        };

        return new()
        {
            Ast = Ast,
            Location = new(Source.Name, start, semicolon.Location.End),
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            IsDeclaration = declaration is not null,
            Declaration = declaration,
            Expression = expression
        };
    }
    
    internal (Declaration?, Expression?)? ParseDeclarationOrExpressionOrNull()
    {
        var declaration = null as Declaration;
        var expression = null as Expression;

        if (Expect(SyntaxFacts.CanBeginDeclarationOrExpression) is not { Kind: var kind }) return null;

        if (SyntaxFacts.CanBeginDeclaration.Contains(kind))
        {
            declaration = ParseDeclaration();
        }
        else if (SyntaxFacts.CanBeginExpression.Contains(kind))
        {
            expression = ParseExpressionOrError();
        }
        else throw new UnreachableException(
            "Kind could begin a statement but neither a declaration nor expression");

        return (declaration, expression);
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

        var expressionBody = null as Expression;
        var blockBody = null as BlockExpression;
        
        if (Current.Kind is TokenKind.OpenBrace)
        {
            blockBody = ParseBlockExpression();
        }
        else
        {
            Expect(TokenKind.EqualsGreaterThan);

            expressionBody = ParseExpressionOrError();
        }

        var end = (blockBody, expressionBody) switch
        {
            (not null, null) => blockBody.Location.End,
            (null, not null) => expressionBody.Location.End,
            // It's impossible for both the expression body and block body to not be null.
            _ => throw new UnreachableException()
        };

        return new()
        {
            Ast = Ast,
            Location = new(Source.Name, func.Location.Start, end),
            Identifier = identifier,
            Parameters = parameters,
            ExpressionBody = expressionBody,
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

        return new()
        {
            Ast = Ast,
            Location = new(Source.Name, let.Location.Start, expression.Location.End),
            IsMutable = isMutable,
            Identifier = identifier,
            Expression = expression
        };
    }
}
