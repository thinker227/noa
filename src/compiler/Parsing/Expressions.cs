using System.Collections.Frozen;
using System.Globalization;
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
            if (!kindsSet.Contains(parser.Current.Kind)) return parser.ParseExpressionOrError(precedence + 1);

            var kindToken = parser.Advance();
            var kind = kindToken.Kind.ToUnaryKind() 
                    ?? throw new InvalidOperationException($"{kindToken.Kind} cannot be converted into a unary kind");
        
            // Parse the same precedence to allow things like !!!x
            var operand = parser.ParseExpressionOrError(precedence);

            return new UnaryExpression()
            {
                Ast = parser.Ast,
                Location = new(parser.Source.Name, kindToken.Location.Start, operand.Location.End),
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
                    Location = new(parser.Source.Name, result.Location.Start, right.Location.End),
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
                Location = new(parser.Source.Name, result.Location.Start, right.Location.End),
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
                Location = new(Source.Name, expression.Location.Start, closeParen.Location.End),
                Target = expression,
                Arguments = arguments
            };
        }

        return expression;
    }

    internal Expression ParsePrimaryExpression()
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
                    Ast = Ast,
                    Location = new(Source.Name, loop.Location.Start, block.Location.End),
                    LoopKeyword = loop,
                    Block = block
                };
            }

        case TokenKind.Return:
            {
                var @return = Advance();

                var expression = ParseExpressionOrNull();

                return new ReturnExpression()
                {
                    Ast = Ast,
                    Location = expression is not null
                        ? new Location(Source.Name, @return.Location.Start, expression.Location.End)
                        : @return.Location,
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
                    Location = expression is not null
                        ? new Location(Source.Name, @break.Location.Start, expression.Location.End)
                        : @break.Location,
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
                    Location = @continue.Location
                };
            }

        case TokenKind.Name:
            {
                var identifier = Advance();

                return new IdentifierExpression()
                {
                    Ast = Ast,
                    Location = identifier.Location,
                    Identifier = identifier.Text
                };
            }

        case TokenKind.True or TokenKind.False:
            {
                var @bool = Advance();

                return new BoolExpression()
                {
                    Ast = Ast,
                    Location = @bool.Location,
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
                        Location = number.Location,
                        Value = value
                    };
                }

                var diagnostic = ParseDiagnostics.LiteralTooLarge.Format(number.Text, number.Location);
                ReportDiagnostic(diagnostic);
                    
                return new ErrorExpression()
                {
                    Ast = Ast,
                    Location = number.Location
                };
            }
        
        default:
            return new ErrorExpression()
            {
                Ast = Ast,
                Location = Location.FromLength(Source.Name, Current.Location.Start, 0)
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
            Location = new(Source.Name, openBrace.Location.Start, closeBrace.Location.End),
            Statements = statements,
            TrailingExpression = trailingExpression
        };
    }

    internal IfExpression ParseIfExpression()
    {
        var @if = Expect(TokenKind.If);

        var condition = ParseExpressionOrError();

        var ifTrue = ParseBlockExpression();

        var @else = Expect(TokenKind.Else);

        var ifFalse = ParseBlockExpression();

        return new()
        {
            Ast = Ast,
            Location = new(Source.Name, @if.Location.Start, ifFalse.Location.End),
            IfKeyword = @if,
            Condition = condition,
            IfTrue = ifTrue,
            ElseKeyword = @else,
            IfFalse = ifFalse
        };
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
