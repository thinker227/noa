using Noa.Compiler.Syntax.Green;
using TextMappingUtils;
using TokenKind = Noa.Compiler.Syntax.TokenKind;

namespace Noa.Compiler.Parsing;

internal sealed partial class Parser
{
    internal (SyntaxList<StatementSyntax>, ExpressionSyntax?) ParseBlock(
        bool allowTrailingExpression,
        TokenKind endKind,
        IReadOnlySet<TokenKind> synchronizationTokens)
    {
        var statements = ImmutableArray.CreateBuilder<StatementSyntax>();
        var trailingExpression = null as ExpressionSyntax;
        
        while (!AtEnd && Current.Kind != endKind)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var statementOrExpression = ParseStatementOrExpressionOrNull();

            if (statementOrExpression is not var (statement, expression))
            {
                // An unexpected token was encountered.
                ReportDiagnostic(ParseDiagnostics.UnexpectedToken, Current);
                ConsumeUnexpected();
                
                // Try synchronize with the next statement or closing brace.
                Synchronize(synchronizationTokens);

                continue;
            }
            
            if (statement is not null)
            {
                // A statement expecting a semicolon is dependent on the syntax of each kind of statement.
                // No semicolon should be expected here.

                statements.Add(statement);
                continue;
            }

            // If the statement is null then the expression should never be null.
            if (expression is null) throw new UnreachableException();

            // If the token after the expression is not a semicolon, cannot start another statement,
            // and is allowed as a trailing expression, then it's most likely a trailing expression.
            // We need to check whether the expression is allowed as a trailing expression because
            // the expression might be an if expression without an else clause, which is only allowed
            // as an expression statement.
            if (allowTrailingExpression &&
                expression.IsAllowedAsTrailingExpression() &&
                Current.Kind is not TokenKind.Semicolon &&
                !SyntaxFacts.CanBeginStatement.Contains(Current.Kind))
            {
                if (Current.Kind == endKind)
                {
                    trailingExpression = expression;
                    break;
                }

                // If the current token is not a closing brace, then there's an unexpected token here.

                ReportDiagnostic(ParseDiagnostics.UnexpectedToken, Current);
                ConsumeUnexpected();

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

            if (!expression.IsExpressionStatement() && expression is not ErrorExpressionSyntax)
            {
                ReportDiagnostic(ParseDiagnostics.InvalidExpressionStatement, expression);
            }
            
            // If the expression is a control flow expression statement then we don't expect a semicolon.
            if (expression.IsControlFlowExpressionStatement() && expression is not ErrorExpressionSyntax)
            {
                statements.Add(new FlowControlStatementSyntax()
                {
                    Expression = expression
                });
            }
            else
            {
                var semicolon = Expect(TokenKind.Semicolon);

                // If the expression is an error then we have to consume the current token to avoid choking on it.
                if (expression is ErrorExpressionSyntax) Advance();

                statements.Add(new ExpressionStatementSyntax()
                {
                    Expression = expression,
                    Semicolon = semicolon
                });
            }
        }

        return (new(statements.ToImmutable()), trailingExpression);
    }
    
    internal (StatementSyntax?, ExpressionSyntax?)? ParseStatementOrExpressionOrNull()
    {
        // Begin by checking whether the current token can begin a statement at all.
        if (Expect(SyntaxFacts.CanBeginStatement) is not { Kind: var kind }) return null;

        // Try to begin a declaration.
        if (SyntaxFacts.CanBeginDeclaration.Contains(kind))
        {
            return (ParseDeclaration(), null);
        }
        
        // Try parse a flow control expression.
        if (ParseFlowControlExpressionOrNull(FlowControlExpressionContext.Statement) is { } flowControlExpression)
        {
            return (null, flowControlExpression);
        }
        
        var expression = ParseExpressionOrError();

        // Parse an assignment statement if we find a token which is a valid assignment operator.
        if (SyntaxFacts.AssignmentOperator.Contains(Current.Kind))
        {
            return (ContinueParsingAssignmentStatement(expression), null);
        }

        // Finally, just return the expression.
        // We don't make this into an expression statement because the expression may be used as a trailing expression.
        return (null, expression);
    }

    private AssignmentStatementSyntax ContinueParsingAssignmentStatement(ExpressionSyntax target)
    {
        var operatorToken = Advance();

        var value = ParseExpressionOrError();

        var semicolon = Expect(TokenKind.Semicolon);

        if (!target.IsValidLValue())
        {
            ReportDiagnostic(ParseDiagnostics.InvalidLValue, target);
        }

        return new AssignmentStatementSyntax()
        {
            Target = target,
            Operator = operatorToken,
            Value = value,
            Semicolon = semicolon
        };
    }
}
