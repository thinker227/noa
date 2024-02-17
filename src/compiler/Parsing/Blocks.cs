using Noa.Compiler.Nodes;

namespace Noa.Compiler.Parsing;

internal sealed partial class Parser
{
    internal (ImmutableArray<Statement>, Expression?) ParseBlock(
        bool allowTrailingExpression,
        TokenKind endKind,
        IReadOnlySet<TokenKind> synchronizationTokens)
    {
        var statements = ImmutableArray.CreateBuilder<Statement>();
        var trailingExpression = null as Expression;
        
        while (!AtEnd && Current.Kind != endKind)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var declarationOrExpression = ParseDeclarationOrExpressionOrNull();

            if (declarationOrExpression is not var (declaration, expression))
            {
                // An unexpected token was encountered.
                var diagnostic = ParseDiagnostics.UnexpectedToken.Format(Current, Current.Location);
                ReportDiagnostic(diagnostic);
                
                // Try synchronize with the next statement or closing brace.
                Synchronize(synchronizationTokens);

                continue;
            }
            
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (declaration is not null)
            {
                // A declaration expecting a semicolon is dependent on the syntax of each kind of declaration.

                statements.Add(new()
                {
                    Ast = Ast,
                    Location = declaration.Location,
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
            if (allowTrailingExpression &&
                Current.Kind is not TokenKind.Semicolon &&
                !SyntaxFacts.CanBeginDeclarationOrExpression.Contains(Current.Kind))
            {
                if (Current.Kind == endKind)
                {
                    trailingExpression = expression;
                    break;
                }

                // If the current token is not a closing brace, then there's an unexpected token here.

                var diagnostic = ParseDiagnostics.UnexpectedToken.Format(Current, Current.Location);
                ReportDiagnostic(diagnostic);

                // Try synchronize with the next statement or closing brace.
                Synchronize(synchronizationTokens);

                // If we find a closing token then the expression was a trailing expression and we're done.
                if (Current.Kind == endKind)
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

            if (!expression.IsExpressionStatement())
            {
                var diagnostic = ParseDiagnostics.InvalidExpressionStatement.Format(expression.Location);
                ReportDiagnostic(diagnostic);
            }
            
            // If the expression is a control flow expression statement then we don't expect a semicolon.
            var semicolon = expression.IsControlFlowExpressionStatement()
                ? null as Token?
                : Expect(TokenKind.Semicolon);

            var end = semicolon?.Location.End ?? expression.Location.End;
            
            statements.Add(new()
            {
                Ast = Ast,
                Location = new(Source.Name, expression.Location.Start, end),
                IsDeclaration = false,
                Declaration = null,
                Expression = expression
            });
        }

        return (statements.ToImmutable(), trailingExpression);
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
}
