using System.Collections.Frozen;
using Noa.Compiler.Nodes;

namespace Noa.Compiler.Parsing;

internal sealed partial class Parser
{
    private delegate Expression ExpressionParser(Parser parser, int precedence);

    /// <summary>
    /// Creates a unary prefix expression parser.
    /// </summary>
    private static ExpressionParser PrefixUnaryParser(params TokenKind[] kinds)
    {
        var kindsSet = kinds.ToFrozenSet();
        
        return (parser, precedence) =>
        {
            if (!kindsSet.Contains(parser.current.Kind)) return parser.ParseExpressionOrError(precedence + 1);

            var kindToken = parser.Advance();
            var kind = kindToken.Kind.ToUnaryKind() 
                    ?? throw new InvalidOperationException($"{kindToken.Kind} cannot be converted into a unary kind");
        
            // Parse the same precedence to allow things like !!!x
            var operand = parser.ParseExpressionOrError(precedence);

            return new UnaryExpression()
            {
                Ast = parser.ast,
                Location = new(parser.source.Name, kindToken.Location.Start, operand.Location.End),
                Kind = kind,
                Operand = operand
            };
        };
    }

    /// <summary>
    /// Creates a left-associative binary expression parser.
    /// </summary>
    private static ExpressionParser LeftBinaryParser(params TokenKind[] kinds)
    {
        var kindsSet = kinds.ToFrozenSet();
        
        return (parser, precedence) =>
        {
            var result = parser.ParseExpressionOrError(precedence + 1);

            while (!parser.AtEnd && kindsSet.Contains(parser.current.Kind))
            {
                var kindToken = parser.Advance();
                var kind = kindToken.Kind.ToBinaryKind()
                    ?? throw new InvalidOperationException($"{kindToken.Kind} cannot be converted into a binary kind");
            
                var right = parser.ParseExpressionOrError(precedence + 1);

                result = new BinaryExpression()
                {
                    Ast = parser.ast,
                    Location = new(parser.source.Name, result.Location.Start, right.Location.End),
                    Left = result,
                    Kind = kind,
                    Right = right
                };
            }

            return result;
        };
    }

    /// <summary>
    /// Creates a right-associative binary expression parser.
    /// </summary>
    private static ExpressionParser RightBinaryParser(params TokenKind[] kinds)
    {
        var kindsSet = kinds.ToFrozenSet();

        return (parser, precedence) =>
        {
            var result = parser.ParseExpressionOrError(precedence + 1);

            if (!kindsSet.Contains(parser.current.Kind)) return result;
            
            var kindToken = parser.Advance();
            var kind = kindToken.Kind.ToBinaryKind()
                ?? throw new InvalidOperationException($"{kindToken.Kind} cannot be converted into a binary kind");

            var right = parser.ParseExpressionOrError(precedence);

            return new BinaryExpression()
            {
                Ast = parser.ast,
                Location = new(parser.source.Name, result.Location.Start, right.Location.End),
                Left = result,
                Kind = kind,
                Right = right
            };
        };
    }

    private readonly ExpressionParser equalityExpressionParser = LeftBinaryParser(
        TokenKind.EqualsEquals,
        TokenKind.BangEquals);

    private readonly ExpressionParser relationalExpressionParser = LeftBinaryParser(
        TokenKind.LessThan,
        TokenKind.GreaterThan,
        TokenKind.LessThanEquals,
        TokenKind.GreaterThanEquals);

    private readonly ExpressionParser termExpressionParser = LeftBinaryParser(
        TokenKind.Plus,
        TokenKind.Dash);

    private readonly ExpressionParser factorExpressionParser = LeftBinaryParser(
        TokenKind.Star,
        TokenKind.Slash);

    private readonly ExpressionParser unaryExpressionParser = PrefixUnaryParser(
        TokenKind.Plus,
        TokenKind.Dash);

    private Expression ParseCallExpression(int precedence)
    {
        var expression = ParseExpressionOrError(precedence + 1);

        if (current.Kind is not TokenKind.OpenParen) return expression;

        Advance();

        var arguments = ParseSeparatedList(
            TokenKind.Comma,
            true,
            ParseExpressionOrError,
            TokenKind.CloseParen);

        var closeParen = Expect(TokenKind.CloseParen);

        return new CallExpression()
        {
            Ast = ast,
            Location = new(source.Name, expression.Location.Start, closeParen.Location.End),
            Target = expression,
            Arguments = arguments
        };
    }

    private Expression ParsePrimaryExpression()
    {
        switch (Expect(SyntaxFacts.CanBeginPrimaryExpression)?.Kind)
        {
        case TokenKind.OpenBrace:
            return ParseBlockExpression();
        
        case TokenKind.OpenParen:
            return ParseParenthesizedOrLambdaExpression();
        
        case TokenKind.If:
            return ParseIfExpression();

        case TokenKind.Loop:
            {
                var loop = Advance();

                var block = ParseBlockExpression();

                return new LoopExpression()
                {
                    Ast = ast,
                    Location = new(source.Name, loop.Location.Start, block.Location.End),
                    Block = block
                };
            }

        case TokenKind.Return:
            {
                var @return = Advance();

                var expression = ParseExpressionOrNull();

                return new ReturnExpression()
                {
                    Ast = ast,
                    Location = expression is not null
                        ? new Location(source.Name, @return.Location.Start, expression.Location.End)
                        : @return.Location,
                    Expression = expression
                };
            }

        case TokenKind.Break:
            {
                var @break = Advance();

                var expression = ParseExpressionOrNull();

                return new BreakExpression()
                {
                    Ast = ast,
                    Location = expression is not null
                        ? new Location(source.Name, @break.Location.Start, expression.Location.End)
                        : @break.Location,
                    Expression = expression
                };
            }

        case TokenKind.Continue:
            {
                var @continue = Advance();

                return new ContinueExpression()
                {
                    Ast = ast,
                    Location = @continue.Location
                };
            }

        case TokenKind.Name:
            {
                var identifier = Advance();

                return new IdentifierExpression()
                {
                    Ast = ast,
                    Location = identifier.Location,
                    Identifier = identifier.Text
                };
            }

        case TokenKind.True or TokenKind.False:
            {
                var @bool = Advance();

                return new BoolExpression()
                {
                    Ast = ast,
                    Location = @bool.Location,
                    Value = @bool.Kind is TokenKind.True
                };
            }

        case TokenKind.Number:
            {
                var number = Advance();

                return new NumberExpression()
                {
                    Ast = ast,
                    Location = number.Location,
                    // Todo: report an error if the literal is too large.
                    Value = int.Parse(number.Text)
                };
            }
        
        default:
            return new ErrorExpression()
            {
                Ast = ast,
                Location = Location.FromLength(source.Name, current.Location.Start, 0)
            };
        }
    }

    private BlockExpression ParseBlockExpression()
    {
        var openBrace = Expect(TokenKind.OpenBrace);

        var statements = ImmutableArray.CreateBuilder<Statement>();
        var trailingExpression = null as Expression;
        
        while (!AtEnd && current.Kind is not TokenKind.CloseBrace)
        {
            var declarationOrExpression = ParseDeclarationOrExpressionOrNull();

            if (declarationOrExpression is not var (declaration, expression))
            {
                // An unexpected token was encountered.
                diagnostics.Add(new(
                    $"Unexpected {current.Kind.ToDisplayString()} token",
                    Severity.Error,
                    current.Location));
                
                // Try synchronize with the next statement or closing brace.
                while (!AtEnd && !SyntaxFacts.BlockExpressionSynchronize.Contains(current.Kind)) Advance();

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
                    Ast = ast,
                    Location = new(source.Name, declaration.Location.Start, semicolon.Location.End),
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
            if (current.Kind is not TokenKind.Semicolon &&
                !SyntaxFacts.CanBeginDeclarationOrExpression.Contains(current.Kind))
            {
                if (current.Kind is TokenKind.CloseBrace)
                {
                    trailingExpression = expression;
                    break;
                }

                // If the current token is not a closing brace, then there's an unexpected token here.
                
                diagnostics.Add(new(
                    $"Unexpected {current.Kind.ToDisplayString()} token",
                    Severity.Error,
                    current.Location));

                // Try synchronize with the next statement or closing brace.
                while (!AtEnd && !SyntaxFacts.BlockExpressionSynchronize.Contains(current.Kind)) Advance();

                // If we find a closing brace then the expression was a trailing expression and we're done.
                if (current.Kind is TokenKind.CloseBrace)
                {
                    trailingExpression = expression;
                    break;
                }

                if (!SyntaxFacts.CanBeginDeclarationOrExpression.Contains(current.Kind))
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
                Ast = ast,
                Location = new(source.Name, expression.Location.Start, semicolon.Location.End),
                IsDeclaration = false,
                Declaration = null,
                Expression = expression
            });
        }
        
        var closeBrace = Expect(TokenKind.CloseBrace);

        return new()
        {
            Ast = ast,
            Location = new(source.Name, openBrace.Location.Start, closeBrace.Location.End),
            Statements = statements.ToImmutable(),
            TrailingExpression = trailingExpression
        };
    }

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
    
    private Expression ParseParenthesizedOrLambdaExpression()
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

    private Expression ContinueParsingParenthesizedExpression(
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

    private Expression CreateParenthesizedExpression(
        Token openParen,
        ImmutableArray<Expression> expressions,
        Token closeParen)
    {
        if (expressions.Length == 0)
        {
            // () has no syntactic meaning.
            // Todo: report an error expecting any expression starter here.
            throw new NotImplementedException();
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

    private IfExpression ParseIfExpression()
    {
        var @if = Expect(TokenKind.If);

        var condition = ParseExpressionOrError();

        var ifTrue = ParseBlockExpression();

        Expect(TokenKind.Else);

        var ifFalse = ParseBlockExpression();

        return new()
        {
            Ast = ast,
            Location = new(source.Name, @if.Location.Start, ifFalse.Location.End),
            Condition = condition,
            IfTrue = ifTrue,
            IfFalse = ifFalse
        };
    }

    /// <summary>
    /// Parses an expression or returns an error.
    /// </summary>
    private Expression ParseExpressionOrError() =>
        ParseExpressionOrError(0);
    
    private Expression ParseExpressionOrError(int precedence) => precedence switch
    {
        0 => equalityExpressionParser(this, precedence),
        1 => relationalExpressionParser(this, precedence),
        2 => termExpressionParser(this, precedence),
        3 => factorExpressionParser(this, precedence),
        4 => unaryExpressionParser(this, precedence),
        5 => ParseCallExpression(precedence),
        6 => ParsePrimaryExpression(),
        _ => throw new UnreachableException()
    };

    /// <summary>
    /// Parses an expression or returns null if the current token cannot start an expression.
    /// </summary>
    private Expression? ParseExpressionOrNull() =>
        SyntaxFacts.CanBeginExpression.Contains(current.Kind)
            ? ParseExpressionOrError()
            : null;
}
