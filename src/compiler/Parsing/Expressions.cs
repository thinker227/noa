using System.Collections.Frozen;
using System.Globalization;
using Noa.Compiler.Nodes;
using TextMappingUtils;

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
            if (!kindsSet.Contains(parser.Current.Kind)) return parser.ParseExpressionOrError(precedence + 1);

            var kindToken = parser.Advance();
            var kind = kindToken.Kind.ToUnaryKind() 
                    ?? throw new InvalidOperationException($"{kindToken.Kind} cannot be converted into a unary kind");
        
            // Parse the same precedence to allow things like !!!x
            var operand = parser.ParseExpressionOrError(precedence);

            return new UnaryExpression()
            {
                Ast = parser.Ast,
                Span = TextSpan.Between(kindToken.Span, operand.Span),
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

            while (!parser.AtEnd && kindsSet.Contains(parser.Current.Kind))
            {
                parser.cancellationToken.ThrowIfCancellationRequested();
                
                var kindToken = parser.Advance();
                var kind = kindToken.Kind.ToBinaryKind()
                    ?? throw new InvalidOperationException($"{kindToken.Kind} cannot be converted into a binary kind");
            
                var right = parser.ParseExpressionOrError(precedence + 1);

                result = new BinaryExpression()
                {
                    Ast = parser.Ast,
                    Span = TextSpan.Between(result.Span, right.Span),
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

            if (!kindsSet.Contains(parser.Current.Kind)) return result;
            
            var kindToken = parser.Advance();
            var kind = kindToken.Kind.ToBinaryKind()
                ?? throw new InvalidOperationException($"{kindToken.Kind} cannot be converted into a binary kind");

            var right = parser.ParseExpressionOrError(precedence);

            return new BinaryExpression()
            {
                Ast = parser.Ast,
                Span = TextSpan.Between(result.Span, right.Span),
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

    internal Expression ParseCallExpression(int precedence)
    {
        var expression = ParseExpressionOrError(precedence + 1);

        while (Current.Kind is TokenKind.OpenParen)
        {
            Advance();
            
            var arguments = ParseSeparatedList(
                TokenKind.Comma,
                true,
                ParseExpressionOrError,
                TokenKind.CloseParen);

            var closeParen = Expect(TokenKind.CloseParen);
            
            expression = new CallExpression()
            {
                Ast = Ast,
                Span = TextSpan.Between(expression.Span, closeParen.Span),
                Target = expression,
                Arguments = arguments
            };
        }

        return expression;
    }

    internal Expression ParsePrimaryExpression()
    {
        if (ParseFlowControlExpressionOrNull(FlowControlExpressionContext.Expression) is { } flowControlExpression)
            return flowControlExpression;
        
        switch (Expect(SyntaxFacts.CanBeginPrimaryExpression)?.Kind)
        {
        case TokenKind.OpenParen:
            return ParseParenthesizedOrLambdaExpression();

        case TokenKind.Return:
            {
                var @return = Advance();

                var expression = ParseExpressionOrNull();

                return new ReturnExpression()
                {
                    Ast = Ast,
                    Span = TextSpan.Between(@return.Span, expression?.Span),
                    ReturnKeyword = @return,
                    Expression = expression
                };
            }

        case TokenKind.Break:
            {
                var @break = Advance();

                var expression = ParseExpressionOrNull();

                return new BreakExpression()
                {
                    Ast = Ast,
                    Span = TextSpan.Between(@break.Span, expression?.Span),
                    BreakKeyword = @break,
                    Expression = expression
                };
            }

        case TokenKind.Continue:
            {
                var @continue = Advance();

                return new ContinueExpression()
                {
                    Ast = Ast,
                    Span = @continue.Span
                };
            }

        case TokenKind.Name:
            {
                var identifier = Advance();

                return new IdentifierExpression()
                {
                    Ast = Ast,
                    Span = identifier.Span,
                    Identifier = identifier.Text
                };
            }

        case TokenKind.True or TokenKind.False:
            {
                var @bool = Advance();

                return new BoolExpression()
                {
                    Ast = Ast,
                    Span = @bool.Span,
                    Value = @bool.Kind is TokenKind.True
                };
            }

        case TokenKind.Number:
            {
                var number = Advance();

                if (int.TryParse(number.Text, NumberStyles.None, CultureInfo.InvariantCulture, out var value))
                {
                    return new NumberExpression()
                    {
                        Ast = Ast,
                        Span = number.Span,
                        Value = value
                    };
                }

                ReportDiagnostic(ParseDiagnostics.LiteralTooLarge, number.Text, number.Span);
                    
                return new ErrorExpression()
                {
                    Ast = Ast,
                    Span = number.Span
                };
            }
        
        case TokenKind.String:
            {
                // Todo: interpolation

                var str = Advance();

                var text = ParseStringText(str);

                return new StringExpression()
                {
                    Ast = Ast,
                    Span = str.Span,
                    Parts = [
                        new TextStringPart()
                        {
                            Ast = Ast,
                            Span = str.Span,
                            Text = text
                        }
                    ]
                };
            }
        
        default:
            return new ErrorExpression()
            {
                Ast = Ast,
                Span = TextSpan.FromLength(Current.Span.Start, 0)
            };
        }
    }
    
    internal BlockExpression ParseBlockExpression()
    {
        var openBrace = Expect(TokenKind.OpenBrace);

        var (statements, trailingExpression) = ParseBlock(
            allowTrailingExpression: true,
            endKind: TokenKind.CloseBrace,
            synchronizationTokens: SyntaxFacts.BlockExpressionSynchronize);
        
        var closeBrace = Expect(TokenKind.CloseBrace);

        return new()
        {
            Ast = Ast,
            Span = TextSpan.Between(openBrace.Span, closeBrace.Span),
            Statements = statements,
            TrailingExpression = trailingExpression
        };
    }

    internal Expression? ParseFlowControlExpressionOrNull(FlowControlExpressionContext ctx)
    {
        switch (Current.Kind)
        {
        case TokenKind.OpenBrace:
            return ParseBlockExpression();

        case TokenKind.If:
            {
                var @if = Expect(TokenKind.If);

                var condition = ParseExpressionOrError();

                var ifTrue = ParseBlockExpression();

                // Parse an else clause if the current token is an else keyword
                // or the context is an expression, in which case the else clause is required.
                var elseClause = null as ElseClause;
                if (Current.Kind is TokenKind.Else || ctx is FlowControlExpressionContext.Expression)
                {
                    // In case the else clause is omitted and the context is an expression,
                    // report an additional little informational error.
                    if (Current.Kind is not TokenKind.Else && ctx is FlowControlExpressionContext.Expression)
                    {
                        ReportDiagnostic(ParseDiagnostics.ElseOmitted, @if.Span);
                    }
                    
                    var @else = Expect(TokenKind.Else);
            
                    var ifFalse = ParseBlockExpression();

                    elseClause = new()
                    {
                        Ast = Ast,
                        Span = TextSpan.Between(@else.Span, ifFalse.Span),
                        ElseKeyword = @else,
                        IfFalse = ifFalse
                    };
                }

                return new IfExpression()
                {
                    Ast = Ast,
                    Span = @if.Span with { End = elseClause?.Span.End ?? ifTrue.Span.End },
                    IfKeyword = @if,
                    Condition = condition,
                    IfTrue = ifTrue,
                    Else = elseClause
                };
            }

        case TokenKind.Loop:
            {
                var loop = Advance();

                var block = ParseBlockExpression();

                return new LoopExpression()
                {
                    Ast = Ast,
                    Span = TextSpan.Between(loop.Span, block.Span),
                    LoopKeyword = loop,
                    Block = block
                };
            }
        }
        
        return null;
    }

    /// <summary>
    /// Parses an expression or returns an error.
    /// </summary>
    internal Expression ParseExpressionOrError() =>
        ParseExpressionOrError(0);
    
    internal Expression ParseExpressionOrError(int precedence) => precedence switch
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
    internal Expression? ParseExpressionOrNull() =>
        SyntaxFacts.CanBeginExpression.Contains(Current.Kind)
            ? ParseExpressionOrError()
            : null;
}
