using Noa.Compiler.Nodes;

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
    public static Root Parse(Source source, Ast ast)
    {
        var tokens = Lexer.Lex(source);
        var parser = new Parser(source, ast, tokens);
        return parser.ParseRoot();
    }

    private Root ParseRoot()
    {
        var statements = ImmutableArray.CreateBuilder<Statement>();

        while (!AtEnd)
        {
            var statement = ParseStatementOrNull();
            
            if (statement is not null) statements.Add(statement);
            else
            {
                // An unexpected token was encountered.
                diagnostics.Add(new(
                    $"Unexpected {current.Kind.ToDisplayString()} token",
                    Severity.Error,
                    current.Location));
                
                // Try synchronize with the next statement.
                while (!AtEnd && !SyntaxFacts.CanBeginStatement.Contains(current.Kind)) Advance();
            }
        }

        var endOfFile = Expect(TokenKind.EndOfFile);

        var start = statements.FirstOrDefault()?.Location.Start ?? endOfFile.Location.Start;
        var location = new Location(source.Name, start, endOfFile.Location.End);

        return new()
        {
            Ast = ast,
            Location = location,
            Statements = statements.ToImmutable()
        };
    }

    private Statement? ParseStatementOrNull()
    {
        bool isDeclaration;
        Declaration? declaration;
        Expression? expression;
        int start;

        if (Expect(SyntaxFacts.CanBeginStatement) is not { Kind: var kind }) return null;

        if (SyntaxFacts.CanBeginDeclaration.Contains(kind))
        {
            isDeclaration = true;
            expression = null;
            declaration = ParseDeclaration();
            start = declaration.Location.Start;
        }
        else if (SyntaxFacts.CanBeginExpression.Contains(kind))
        {
            isDeclaration = false;
            declaration = null;
            // Fine to accept error expressions here because we're gonna synchronize after a statement anyway.
            expression = ParseExpressionOrError();
            start = expression.Location.Start;
        }
        else throw new UnreachableException(
            "Kind could begin a statement but neither a declaration nor expression");

        var semicolon = Expect(TokenKind.Semicolon);

        return new()
        {
            Ast = ast,
            Location = new(source.Name, start, semicolon.Location.End),
            IsDeclaration = isDeclaration,
            Declaration = declaration,
            Expression = expression
        };
    }

    private Identifier ParseIdentifier()
    {
        var name = Expect(TokenKind.Name);

        return new()
        {
            Ast = ast,
            Location = name.Location,
            Name = name.Text
        };
    }

    private Declaration ParseDeclaration() => current.Kind switch
    {
        TokenKind.Func => ParseFunctionDeclaration(),
        TokenKind.Let => ParseLetDeclaration(),
        _ => throw new InvalidOperationException(
            $"Cannot parse a declaration starting with {current.Kind}.")
    };

    private FunctionDeclaration ParseFunctionDeclaration()
    {
        throw new NotImplementedException();
    }

    private LetDeclaration ParseLetDeclaration()
    {
        var let = Expect(TokenKind.Let);

        bool isMutable;
        if (current.Kind is TokenKind.Mut)
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
            Ast = ast,
            Location = new(source.Name, let.Location.Start, expression.Location.End),
            IsMutable = isMutable,
            Identifier = identifier,
            Expression = expression
        };
    }
}
