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
            
            var statementOrExpression = ParseStatementOrExpressionOrNull();

            if (statementOrExpression is not var (statement, expression))
            {
                // An unexpected token was encountered.
                var diagnostic = ParseDiagnostics.UnexpectedToken.Format(Current, Current.Location);
                ReportDiagnostic(diagnostic);
                
                // Try synchronize with the next statement or closing brace.
                Synchronize(synchronizationTokens);

                continue;
            }
            
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (statement is not null)
            {
                // A statement expecting a semicolon is dependent on the syntax of each kind of statement.
                // No semicolon should be expected here.

                statements.Add(statement);
                continue;
            }

            // If the statement is null then the expression should never be null.
            if (expression is null) throw new UnreachableException();

            // If the token after the expression is not a semicolon and cannot start another statement,
            // then it's most likely a trailing expression.
            if (allowTrailingExpression &&
                Current.Kind is not TokenKind.Semicolon &&
                !SyntaxFacts.CanBeginStatement.Contains(Current.Kind))
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

                if (!SyntaxFacts.CanBeginStatement.Contains(Current.Kind))
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

            statements.Add(new ExpressionStatement()
            {
                Ast = Ast,
                Location = new(Source.Name, expression.Location.Start, end),
                Expression = expression
            });
        }

        return (statements.ToImmutable(), trailingExpression);
    }
    
    internal (Statement?, Expression?)? ParseStatementOrExpressionOrNull()
    {

        if (Expect(SyntaxFacts.CanBeginStatement) is not { Kind: var kind }) return null;

        if (SyntaxFacts.CanBeginDeclaration.Contains(kind))
        {
            var declaration = ParseDeclaration();
            return (declaration, null);
        }

        if (SyntaxFacts.CanBeginExpression.Contains(kind))
        {
            return ParseExpressionOrAssignmentStatement();
        }
        
        throw new UnreachableException(
            "Kind could begin a statement but neither a declaration nor expression");
    }

    internal (Statement?, Expression?) ParseExpressionOrAssignmentStatement()
    {
        var expression = ParseExpressionOrError();

        if (Current.Kind is not TokenKind.Equals) return (null, expression);
        
        Advance();

        var value = ParseExpressionOrError();

        Expect(TokenKind.Semicolon);

        if (!expression.IsValidLValue())
        {
            var diagnostic = ParseDiagnostics.InvalidLValue.Format(expression.Location);
            ReportDiagnostic(diagnostic);
        }

        var assignment = new AssignmentStatement()
        {
            Ast = Ast,
            Location = new(Source.Name, expression.Location.Start, value.Location.End),
            Target = expression,
            Value = value,
        };
        return (assignment, null);
    }
}
