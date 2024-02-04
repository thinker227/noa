using Noa.Compiler.Nodes;

namespace Noa.Compiler.Parsing;

internal sealed partial class Parser
{
    internal BlockExpression ParseBlockExpression()
    {
        var openBrace = Expect(TokenKind.OpenBrace);

        var statements = ImmutableArray.CreateBuilder<Statement>();
        var trailingExpression = null as Expression;
        
        while (!AtEnd && Current.Kind is not TokenKind.CloseBrace)
        {
            var declarationOrExpression = ParseDeclarationOrExpressionOrNull();

            if (declarationOrExpression is not var (declaration, expression))
            {
                // An unexpected token was encountered.
                var diagnostic = ParseDiagnostics.UnexpectedToken.Format(Current, Current.Location);
                ReportDiagnostic(diagnostic);
                
                // Try synchronize with the next statement or closing brace.
                while (!AtEnd && !SyntaxFacts.BlockExpressionSynchronize.Contains(Current.Kind)) Advance();

                continue;
            }

            Token semicolon;
            
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (declaration is not null)
            {
                // A declaration always expects a semicolon afterwards.
                semicolon = Expect(TokenKind.Semicolon);

                statements.Add(new()
                {
                    Ast = Ast,
                    Location = new(Source.Name, declaration.Location.Start, semicolon.Location.End),
                    IsDeclaration = true,
                    Declaration = declaration,
                    Expression = null
                });
                continue;
            }

            // If the declaration is null then the expression should never be null.
            if (expression is null) throw new UnreachableException();

            // If the token after the expression is not a semicolon and cannot start another
            // declaration or expression, then it's most likely a trailing expression.
            if (Current.Kind is not TokenKind.Semicolon &&
                !SyntaxFacts.CanBeginDeclarationOrExpression.Contains(Current.Kind))
            {
                if (Current.Kind is TokenKind.CloseBrace)
                {
                    trailingExpression = expression;
                    break;
                }

                // If the current token is not a closing brace, then there's an unexpected token here.

                var diagnostic = ParseDiagnostics.UnexpectedToken.Format(Current, Current.Location);
                ReportDiagnostic(diagnostic);

                // Try synchronize with the next statement or closing brace.
                while (!AtEnd && !SyntaxFacts.BlockExpressionSynchronize.Contains(Current.Kind)) Advance();

                // If we find a closing brace then the expression was a trailing expression and we're done.
                if (Current.Kind is TokenKind.CloseBrace)
                {
                    trailingExpression = expression;
                    break;
                }

                if (!SyntaxFacts.CanBeginDeclarationOrExpression.Contains(Current.Kind))
                {
                    // If we stop on a token which isn't a closing brace and cannot start a statement,
                    // then the synchronization has reached the end of the input.
                    break;
                }
                
                // If we find a token which can begin another statement,
                // then the previous lack of a semicolon was probably a mistake.
                // Continue parsing statements.
            }
            
            // The expression is an expression statement.
            
            semicolon = Expect(TokenKind.Semicolon);

            statements.Add(new()
            {
                Ast = Ast,
                Location = new(Source.Name, expression.Location.Start, semicolon.Location.End),
                IsDeclaration = false,
                Declaration = null,
                Expression = expression
            });
        }
        
        var closeBrace = Expect(TokenKind.CloseBrace);

        return new()
        {
            Ast = Ast,
            Location = new(Source.Name, openBrace.Location.Start, closeBrace.Location.End),
            Statements = statements.ToImmutable(),
            TrailingExpression = trailingExpression
        };
    }
}
