using Noa.Compiler.Syntax.Green;
using TextMappingUtils;
using TokenKind = Noa.Compiler.Syntax.TokenKind;

namespace Noa.Compiler.Parsing;

internal sealed partial class Parser
{
    // The syntax (a, b, c) is ambiguous because it could either be a lambda parameter list or a tuple expression.
    // We solve this by first attempting to parse it as a lambda and backtracking to the start of the list
    // if we encounter something which looks more like an expression than a parameter.
    
    internal ExpressionSyntax ParseParenthesizedOrLambdaExpression()
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

    internal LambdaExpressionSyntax? ParseLambdaExpressionOrNull(Token openParen)
    {
        var lockedIn = false;
        var parameters = new List<ParameterSyntax>();
        var separators = new List<Token>();
        
        while (!AtEnd)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            // Check whether we've hit the end of the parameter list.
            // Checking this at the start of the loop also permits trailing commas.
            if (Current.Kind is TokenKind.CloseParen or TokenKind.EqualsGreaterThan) break;
            
            var mutToken = null as Token;
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
            
            var identifier = Expect(TokenKind.Name);
            
            parameters.Add(new()
            {
                Name = identifier,
                Mut = mutToken
            });
            
            // Check whether we've hit the end of the parameter list again before trying to find a comma.
            if (Current.Kind is TokenKind.CloseParen or TokenKind.EqualsGreaterThan) break;

            var separator = Expect(TokenKind.Comma);
            separators.Add(separator);
        }

        var closeParen = Expect(TokenKind.CloseParen);

        if (!lockedIn && Current.Kind is not TokenKind.EqualsGreaterThan)
        {
            // Turns out we haven't been parsing a lambda.
            return null;
        }

        var arrow = Expect(TokenKind.EqualsGreaterThan);

        var body = ParseExpressionOrError();

        return new()
        {
            Parameters = new()
            {
                OpenParen = openParen,
                Parameters = SeparatedSyntaxList<ParameterSyntax>.Create(parameters, separators),
                CloseParen = closeParen
            },
            Arrow = arrow,
            Expression = body
        };
    }

    internal ExpressionSyntax ParseParenthesizedExpression(Token openParen)
    {
        var expressions = ParseSeparatedList(
            TokenKind.Comma,
            false,
            ParseExpressionOrError,
            TokenKind.CloseParen);

        var closeParen = Expect(TokenKind.CloseParen);

        if (expressions.NodesCount == 0)
        {
            return new NilExpressionSyntax()
            {
                OpenParen = openParen,
                CloseParen = closeParen
            };
        }

        // If the expression list has a trailing separator then it's most likely intended to be a tuple.
        if (expressions.NodesCount == 1 && !expressions.HasTrailingSeparator)
            return expressions.GetNodeAt(0);

        var tuple = new TupleExpressionSyntax()
        {
            OpenParen = openParen,
            Expressions = expressions,
            CloseParen = closeParen
        };
        
        // Todo: this should probably be moved to a standalone analyzer since it doesn't really belong in the parser.
        ReportDiagnostic(MiscellaneousDiagnostics.TuplesUnsupported, tuple);

        return tuple;
    }
}
