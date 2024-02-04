using Noa.Compiler.Nodes;

namespace Noa.Compiler.Parsing;

internal sealed partial class Parser
{
    // THE THREE HORSEMETHODS OF PARENTHESIZED EXPRESSIONS:
    // The syntax (a, b, c) is ambiguous, because it may either be a tuple expression,
    // or the parameter list of a lambda expression. We solve this using ParseParenthesizedOrLambdaExpression
    // which assumes what it's parsing may either be a lambda or a parenthesized expression, then depending
    // on the contents of the parameter list may switch to parsing a parenthesized expression, or continue
    // parsing a lambda expression. If the method encounters an expression which isn't an identifier,
    // it will switch to ContinueParsingParenthesizedExpression which takes over from parsing the now expression list,
    // however if it encounters a mut token, it will lock into parsing a lambda.
    // CreateParenthesizedExpression handles turning a complete set of parens and expressions
    // into an appropriate expression.
    
    internal Expression ParseParenthesizedOrLambdaExpression()
    {
        var openParen = Expect(TokenKind.OpenParen);

        var lockedIntoLambda = false;
        var parameters = ImmutableArray.CreateBuilder<Parameter>();
        
        while (!AtEnd && current.Kind is not TokenKind.CloseParen)
        {
            if (!lockedIntoLambda &&
                !SyntaxFacts.CanBeginParameter.Contains(current.Kind) &&
                SyntaxFacts.CanBeginExpression.Contains(current.Kind))
            {
                // We encountered something which cannot begin a parameter but can begin an expression.
                // Switch into parsing an expression list instead.
                var expressions = ToExpressionList(parameters);
                return ContinueParsingParenthesizedExpression(openParen, expressions);
            }

            var mutToken = null as Token?;
            var isMutable = false;
            if (current.Kind is TokenKind.Mut)
            {
                mutToken = Advance();
                isMutable = true;
                
                // If we encounter a mut token, we know it can only be a lambda we're parsing.
                lockedIntoLambda = true;
            }

            var identifier = ParseIdentifier();

            var start = mutToken?.Location.Start ?? identifier.Location.Start;
            
            parameters.Add(new()
            {
                Ast = ast,
                Location = new(source.Name, start, identifier.Location.End),
                IsMutable = isMutable,
                Identifier = identifier
            });

            // If we find the closing paren, we're done parsing the parameter list.
            // Also check for a lambda arrow and any expressions starters
            // to avoid freaking out over a missing closing paren.
            if (current.Kind is TokenKind.CloseParen or TokenKind.EqualsGreaterThan ||
                SyntaxFacts.CanBeginExpression.Contains(current.Kind))
                break;

            Expect(TokenKind.Comma);
            
            // Note: because we check for a closing paren in the loop condition,
            // we're also allowing trailing commas.
        }

        var closeParen = Expect(TokenKind.CloseParen);

        // If the lambda arrow is missing and we're not locked into a lambda expression,
        // this is a parenthesized expression.
        if (!lockedIntoLambda && current.Kind is not TokenKind.EqualsGreaterThan)
        {
            var expressions = ToExpressionList(parameters);
            return CreateParenthesizedExpression(openParen, expressions, closeParen);
        }
        
        Expect(TokenKind.EqualsGreaterThan);

        var body = ParseExpressionOrError();

        return new LambdaExpression()
        {
            Ast = ast,
            Location = new(source.Name, openParen.Location.Start, body.Location.End),
            Parameters = parameters.ToImmutable(),
            Body = body
        };

        static ImmutableArray<Expression> ToExpressionList(ImmutableArray<Parameter>.Builder parameters)
        {
            var expressions = ImmutableArray.CreateBuilder<Expression>(parameters.Count);
            foreach (var parameter in parameters)
            {
                expressions.Add(new IdentifierExpression()
                {
                    Ast = parameter.Identifier.Ast,
                    Location = parameter.Identifier.Location,
                    Identifier = parameter.Identifier.Name
                });
            }

            return expressions.ToImmutable();
        }
    }

    internal Expression ContinueParsingParenthesizedExpression(
        Token openParen,
        ImmutableArray<Expression> expressions)
    {
        var restExpressions = ParseSeparatedList(
            TokenKind.Comma,
            false,
            ParseExpressionOrError,
            TokenKind.CloseParen);
        expressions = expressions.AddRange(restExpressions);

        var closeParen = Expect(TokenKind.CloseParen);

        return CreateParenthesizedExpression(openParen, expressions, closeParen);
    }

    internal Expression CreateParenthesizedExpression(
        Token openParen,
        ImmutableArray<Expression> expressions,
        Token closeParen)
    {
        if (expressions.Length == 0)
        {
            // () has no syntactic meaning.
            
            var diagnostic = ParseDiagnostics.ExpectedKinds.Format(SyntaxFacts.CanBeginExpression, closeParen.Location);
            diagnostics.Add(diagnostic);
        }

        // This is just (expr).
        if (expressions.Length == 1) return expressions[0];
        
        return new TupleExpression()
        {
            Ast = ast,
            Location = new(source.Name, openParen.Location.Start, closeParen.Location.End),
            Expressions = expressions
        };
    }
}
