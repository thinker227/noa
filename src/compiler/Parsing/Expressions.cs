using System.Collections.Frozen;
using Noa.Compiler.Syntax.Green;
using TokenKind = Noa.Compiler.Syntax.TokenKind;

namespace Noa.Compiler.Parsing;

internal sealed partial class Parser
{
    private delegate ExpressionSyntax ExpressionParser(Parser parser, int precedence);

    /// <summary>
    /// Creates a unary prefix expression parser.
    /// </summary>
    private static ExpressionParser PrefixUnaryParser(params TokenKind[] kinds)
    {
        var kindsSet = kinds.ToFrozenSet();
        
        return (parser, precedence) =>
        {
            if (!kindsSet.Contains(parser.Current.Kind)) return parser.ParseExpressionOrError(precedence + 1);

            var operatorToken = parser.Advance();
        
            // Parse the same precedence to allow things like !!!x
            var operand = parser.ParseExpressionOrError(precedence);

            return new UnaryExpressionSyntax()
            {
                Operator = operatorToken,
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
                
                var operatorToken = parser.Advance();
            
                var right = parser.ParseExpressionOrError(precedence + 1);

                result = new BinaryExpressionSyntax()
                {
                    Left = result,
                    Operator = operatorToken,
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
            
            var operatorToken = parser.Advance();

            var right = parser.ParseExpressionOrError(precedence);

            return new BinaryExpressionSyntax()
            {
                Left = result,
                Operator = operatorToken,
                Right = right
            };
        };
    }

    private readonly ExpressionParser orExpressionParser = LeftBinaryParser(
        TokenKind.Or);

    private readonly ExpressionParser andExpressionParser = LeftBinaryParser(
        TokenKind.And);

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
        TokenKind.Dash,
        TokenKind.Not);

    internal ExpressionSyntax ParseCallExpression(int precedence)
    {
        var expression = ParseExpressionOrError(precedence + 1);

        while (Current.Kind is TokenKind.OpenParen)
        {
            var openParen = Advance();
            
            var arguments = ParseSeparatedList(
                TokenKind.Comma,
                true,
                ParseExpressionOrError,
                TokenKind.CloseParen);

            var closeParen = Expect(TokenKind.CloseParen);
            
            expression = new CallExpressionSyntax()
            {
                Target = expression,
                OpenParen = openParen,
                Arguments = arguments,
                CloseParen = closeParen
            };
        }

        return expression;
    }

    internal ExpressionSyntax ParsePrimaryExpression()
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

                return new ReturnExpressionSyntax()
                {
                    Return = @return,
                    Value = expression
                };
            }

        case TokenKind.Break:
            {
                var @break = Advance();

                var expression = ParseExpressionOrNull();

                return new BreakExpressionSyntax()
                {
                    Break = @break,
                    Value = expression
                };
            }

        case TokenKind.Continue:
            {
                var @continue = Advance();

                return new ContinueExpressionSyntax()
                {
                    Continue = @continue
                };
            }

        case TokenKind.Name:
            {
                var identifier = Advance();

                return new IdentifierExpressionSyntax()
                {
                    Identifier = identifier
                };
            }

        case TokenKind.True or TokenKind.False:
            {
                var @bool = Advance();

                return new BoolExpressionSyntax()
                {
                    Value = @bool
                };
            }

        case TokenKind.Number:
            {
                var number = Advance();

                return new NumberExpressionSyntax()
                {
                    Value = number
                };
            }
        
        case TokenKind.BeginString:
            return ParseString();
        
        default:
            return new ErrorExpressionSyntax();
        }
    }
    
    internal BlockExpressionSyntax ParseBlockExpression()
    {
        var openBrace = Expect(TokenKind.OpenBrace);

        var (statements, trailingExpression) = ParseBlock(
            allowTrailingExpression: true,
            endKind: TokenKind.CloseBrace,
            synchronizationTokens: SyntaxFacts.BlockExpressionSynchronize);
        
        var closeBrace = Expect(TokenKind.CloseBrace);

        return new()
        {
            OpenBrace = openBrace,
            Block = new()
            {
                Statements = statements,
                TrailingExpression = trailingExpression,
            },
            CloseBrace = closeBrace
        };
    }

    internal ExpressionSyntax? ParseFlowControlExpressionOrNull(FlowControlExpressionContext ctx)
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
                var elseClause = null as ElseClauseSyntax;
                if (Current.Kind is TokenKind.Else || ctx is FlowControlExpressionContext.Expression)
                {
                    // In case the else clause is omitted and the context is an expression,
                    // report an additional little informational error.
                    if (Current.Kind is not TokenKind.Else && ctx is FlowControlExpressionContext.Expression)
                    {
                        ReportDiagnostic(ParseDiagnostics.ElseOmitted, @if);
                    }
                    
                    var @else = Expect(TokenKind.Else);
            
                    var ifFalse = ParseBlockExpression();

                    elseClause = new()
                    {
                        Else = @else,
                        Body = ifFalse
                    };
                }

                return new IfExpressionSyntax()
                {
                    If = @if,
                    Condition = condition,
                    Body = ifTrue,
                    Else = elseClause
                };
            }

        case TokenKind.Loop:
            {
                var loop = Advance();

                var block = ParseBlockExpression();

                return new LoopExpressionSyntax()
                {
                    Loop = loop,
                    Body = block
                };
            }
        }
        
        return null;
    }

    /// <summary>
    /// Parses an expression or returns an error.
    /// </summary>
    internal ExpressionSyntax ParseExpressionOrError() =>
        ParseExpressionOrError(0);
    
    internal ExpressionSyntax ParseExpressionOrError(int precedence) => precedence switch
    {
        0 => orExpressionParser(this, precedence),
        1 => andExpressionParser(this, precedence),
        2 => equalityExpressionParser(this, precedence),
        3 => relationalExpressionParser(this, precedence),
        4 => termExpressionParser(this, precedence),
        5 => factorExpressionParser(this, precedence),
        6 => unaryExpressionParser(this, precedence),
        7 => ParseCallExpression(precedence),
        8 => ParsePrimaryExpression(),
        _ => throw new UnreachableException()
    };

    /// <summary>
    /// Parses an expression or returns null if the current token cannot start an expression.
    /// </summary>
    internal ExpressionSyntax? ParseExpressionOrNull() =>
        SyntaxFacts.CanBeginExpression.Contains(Current.Kind)
            ? ParseExpressionOrError()
            : null;
}
