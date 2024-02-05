using Noa.Compiler.Nodes;

namespace Noa.Compiler.Parsing;

internal sealed partial class Parser
{
    internal Expression ParseParenthesizedOrLambdaExpression()
    {
        var openParen = Expect(TokenKind.OpenParen);

        // Create a state which can be backtracked to in case we fail to parse a lambda.
        var backtrackState = state;
        state = state.Branch();

        var lambda = ParseLambdaExpressionOrNull(openParen);
        if (lambda is not null) return lambda;
        
        // We failed to parse a lambda, backtrack to the previous state.
        state = backtrackState;

        return ParseParenthesizedExpression(openParen);
    }

    internal LambdaExpression? ParseLambdaExpressionOrNull(Token openParen)
    {
        var lockedIn = false;
        var parameters = ImmutableArray.CreateBuilder<Parameter>();
        
        while (!AtEnd)
        {
            // Check whether we've hit the end of the parameter list.
            // Checking this at the start of the loop also permits trailing commas.
            if (Current.Kind is TokenKind.CloseParen or TokenKind.EqualsGreaterThan) break;
            
            var mutToken = null as Token?;
            if (Current.Kind is TokenKind.Mut)
            {
                mutToken = Advance();
                
                // If we've parsed a mut token, then it can only be a lambda that we're parsing.
                lockedIn = true;
            }

            if (!lockedIn &&
                Current.Kind is not TokenKind.Name &&
                SyntaxFacts.CanBeginExpression.Contains(Current.Kind))
            {
                // We found something which can begin an expression and is not a name.
                // Return to switch to parsing a parenthesized expression.
                return null;
            }

            if (Current.Kind is not TokenKind.Name)
            {
                // An unexpected token was encountered.
                var diagnostic = ParseDiagnostics.UnexpectedToken.Format(Current, Current.Location);
                ReportDiagnostic(diagnostic);
            
                // Try synchronize with the next parameter.
                while (!AtEnd && !SyntaxFacts.LambdaParameterListSynchronize.Contains(Current.Kind)) Advance();

                if (Current.Kind is TokenKind.CloseParen or TokenKind.EqualsGreaterThan)
                {
                    // We've synchronized with the end of the parameter list.
                    break;
                }
            }
            
            var identifier = ParseIdentifier();

            var start = mutToken?.Location.Start ?? identifier.Location.Start;
            
            parameters.Add(new()
            {
                Ast = Ast,
                Location = new(Source.Name, start, identifier.Location.End),
                Identifier = identifier,
                IsMutable = mutToken is not null
            });
            
            // Check whether we've hit the end of the parameter list again before trying to find a comma.
            if (Current.Kind is TokenKind.CloseParen or TokenKind.EqualsGreaterThan) break;

            Expect(TokenKind.Comma);
        }

        Expect(TokenKind.CloseParen);

        if (Current.Kind is not TokenKind.EqualsGreaterThan)
        {
            // Turns out we haven't been parsing a lambda.
            return null;
        }

        // Skip =>
        Advance();

        var body = ParseExpressionOrError();

        return new()
        {
            Ast = Ast,
            Location = new(Source.Name, openParen.Location.Start, body.Location.End),
            Parameters = parameters.ToImmutable(),
            Body = body
        };
    }

    internal Expression ParseParenthesizedExpression(Token openParen)
    {
        var expressions = ParseSeparatedList(
            TokenKind.Comma,
            false,
            ParseExpressionOrError,
            TokenKind.CloseParen);

        var closeParen = Expect(TokenKind.CloseParen);

        var parensLocation = new Location(Source.Name, openParen.Location.Start, closeParen.Location.End);

        if (expressions.Length == 0)
        {
            // () is just invalid syntax.
            
            var diagnostic = ParseDiagnostics.ExpectedKinds.Format(
                SyntaxFacts.CanBeginExpression,
                closeParen.Location);
            ReportDiagnostic(diagnostic);

            return new ErrorExpression()
            {
                Ast = Ast,
                Location = parensLocation
            };
        }

        if (expressions.Length == 1) return expressions[0];

        return new TupleExpression()
        {
            Ast = Ast,
            Location = parensLocation,
            Expressions = expressions
        };
    }
}
