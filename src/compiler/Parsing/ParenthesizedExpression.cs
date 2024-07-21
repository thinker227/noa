using Noa.Compiler.Nodes;

namespace Noa.Compiler.Parsing;

internal sealed partial class Parser
{
    // The syntax (a, b, c) is ambiguous because it could either be a lambda parameter list or a tuple expression.
    // We solve this by first attempting to parse it as a lambda and backtracking to the start of the list
    // if we encounter something which looks more like an expression than a parameter.
    
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
            cancellationToken.ThrowIfCancellationRequested();
            
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
                ReportDiagnostic(ParseDiagnostics.UnexpectedToken, Current);
            
                // Try synchronize with the next parameter.
                Synchronize(SyntaxFacts.LambdaParameterListSynchronize);

                if (Current.Kind is TokenKind.CloseParen or TokenKind.EqualsGreaterThan)
                {
                    // We've synchronized with the end of the parameter list.
                    break;
                }
            }
            
            var identifier = ParseIdentifier();

            var start = mutToken?.Span.Start ?? identifier.Span.Start;
            
            parameters.Add(new()
            {
                Ast = Ast,
                Span = identifier.Span with { Start = start },
                Identifier = identifier,
                IsMutable = mutToken is not null
            });
            
            // Check whether we've hit the end of the parameter list again before trying to find a comma.
            if (Current.Kind is TokenKind.CloseParen or TokenKind.EqualsGreaterThan) break;

            Expect(TokenKind.Comma);
        }

        Expect(TokenKind.CloseParen);

        if (!lockedIn && Current.Kind is not TokenKind.EqualsGreaterThan)
        {
            // Turns out we haven't been parsing a lambda.
            return null;
        }

        var arrow = Expect(TokenKind.EqualsGreaterThan);

        var body = ParseExpressionOrError();

        return new()
        {
            Ast = Ast,
            Span = TextSpan.Between(openParen.Span, body.Span),
            Parameters = parameters.ToImmutable(),
            ArrowToken = arrow,
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

        var parensSpan = TextSpan.Between(openParen.Span, closeParen.Span);

        if (expressions.Length == 0)
        {
            return new NilExpression()
            {
                Ast = Ast,
                Span = parensSpan
            };
        }

        if (expressions.Length == 1) return expressions[0];

        var tuple = new TupleExpression()
        {
            Ast = Ast,
            Span = parensSpan,
            Expressions = expressions
        };
        
        // Todo: this should probably be moved to a standalone analyzer since it doesn't really belong in the parser.
        ReportDiagnostic(MiscellaneousDiagnostics.TuplesUnsupported, tuple.Span);

        return tuple;
    }
}
