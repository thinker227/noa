using static Noa.Compiler.Syntax.SyntaxFactory;

namespace Noa.Compiler.Syntax.Tests;

public class SyntaxNavigationTests
{
    [Fact]
    public void GetFirstToken_ReturnsSelf_ForTokenWithoutTriviaTokens()
    {
        var token = Token(TokenKind.True);

        token.GetFirstToken().ShouldBe(token);
    }

    [Fact]
    public void GetFirstToken_ReturnsFirstTriviaToken_ForTokenWithTriviaTokens()
    {
        var unexpected = UnexpectedToken(TokenKind.Mut);
        var token = Token(
            TokenKind.True,
            unexpected,
            Whitespace(" "),
            SkippedToken(TokenKind.Mut),
            Whitespace(" "));

        token.GetFirstToken().ShouldBe(unexpected);
    }

    [Fact]
    public void GetLastToken_ReturnsSelf_ForTokenWithoutTriviaTokens()
    {
        var token = Token(TokenKind.True);

        token.GetLastToken().ShouldBe(token);
    }

    [Fact]
    public void GetLastToken_ReturnsLastTriviaToken_ForTokenWithTriviaTokens()
    {
        var skipped = SkippedToken(TokenKind.Mut);
        var token = Token(
            TokenKind.True,
            UnexpectedToken(TokenKind.Mut),
            Whitespace(" "),
            skipped,
            Whitespace(" "));

        token.GetLastToken().ShouldBe(skipped);
    }

    [Fact]
    public void GetFirstToken_ReturnsSelf_ForUnexpectedToken()
    {
        var unexpected = UnexpectedToken(TokenKind.Mut);

        unexpected.GetFirstToken().ShouldBe(unexpected);
    }

    [Fact]
    public void GetLastToken_ReturnsSelf_ForUnexpectedToken()
    {
        var unexpected = UnexpectedToken(TokenKind.Mut);

        unexpected.GetLastToken().ShouldBe(unexpected);
    }

    [Fact]
    public void GetFirstToken_ReturnsSelf_ForSkippedToken()
    {
        var skipped = SkippedToken(TokenKind.Mut);

        skipped.GetFirstToken().ShouldBe(skipped);
    }

    [Fact]
    public void GetLastToken_ReturnsSelf_ForSkippedToken()
    {
        var skipped = SkippedToken(TokenKind.Mut);

        skipped.GetLastToken().ShouldBe(skipped);
    }

    [Fact]
    public void GetFirstToken_GetsFirstToken_InNode()
    {
        var letToken = Token(TokenKind.Let);
        var node = LetDeclaration(
            letToken,
            null,
            Token(TokenKind.Name, "x"),
            Token(TokenKind.Equals),
            NumberExpression(Token(TokenKind.Number, "1")),
            Token(TokenKind.Semicolon));
        
        node.GetFirstToken().ShouldBe(letToken);
    }

    [Fact]
    public void GetLastToken_GetsFirstToken_InNode()
    {
        var semicolonToken = Token(TokenKind.Semicolon);
        var node = LetDeclaration(
            Token(TokenKind.Let),
            null,
            Token(TokenKind.Name, "x"),
            Token(TokenKind.Equals),
            NumberExpression(Token(TokenKind.Number, "1")),
            semicolonToken);
        
        node.GetLastToken().ShouldBe(semicolonToken);
    }

    [Fact]
    public void GetFirstToken_GetsFirstToken_InChildren()
    {
        var returnToken = Token(TokenKind.Return);
        var node = Root(
            Block(
                [
                    ExpressionStatement(
                        ReturnExpression(
                            returnToken,
                            null),
                        Token(TokenKind.Semicolon))
                ],
                NumberExpression(
                    Token(TokenKind.Number, "1"))),
            Token(TokenKind.EndOfFile));
        
        node.GetFirstToken().ShouldBe(returnToken);
    }

    [Fact]
    public void GetLastToken_GetsLastToken_InChildren()
    {
        var numberToken = Token(TokenKind.Number, "1");
        var node = Root(
            Block(
                [
                    ExpressionStatement(
                        ReturnExpression(
                            Token(TokenKind.Return),
                            null),
                        Token(TokenKind.Semicolon))
                ],
                NumberExpression(numberToken)),
            Token(TokenKind.EndOfFile));
        
        node.GetLastToken().ShouldBe(numberToken);
    }

    [Fact]
    public void GetPreviousToken_ReturnsNull_ForSingleTokenWithoutPreviousToken()
    {
        var token = Token(TokenKind.Name, "uwu");

        token.GetPreviousToken().ShouldBeNull();
    }

    [Fact]
    public void GetPreviousToken_ReturnsNull_ForNestedTokenWithoutPreviousToken()
    {
        var node = Root(
            Block(
                [
                    ExpressionStatement(
                        ReturnExpression(
                            Token(TokenKind.Return),
                            null),
                        Token(TokenKind.Semicolon))
                ],
                null),
            Token(TokenKind.EndOfFile));

        var token =
            ((node.Block.Statements[0] as ExpressionStatementSyntax)!
                .Expression as ReturnExpressionSyntax)!
                    .Return;
        
        token.GetPreviousToken().ShouldBeNull();
    }

    [Fact]
    public void GetPreviousToken_ReturnsLastTriviaToken_ForTokenWithTriviaTokens()
    {
        var skipped = SkippedToken(TokenKind.Mut);
        var node = Token(
            TokenKind.Number,
            "1",
            UnexpectedToken(TokenKind.Mut),
            Whitespace(" "),
            skipped,
            Whitespace(" "));
        
        node.GetPreviousToken().ShouldBe(skipped);
    }

    [Fact]
    public void GetPreviousToken_ReturnsLastTokenInPreviousSiblingInAncestor_ForNodeWithoutPreviousTokenInSelf()
    {
        var returnToken = Token(TokenKind.Return);
        var node = Block(
            [
                ExpressionStatement(
                    ReturnExpression(
                        Token(TokenKind.Return),
                        null),
                    returnToken),
                ExpressionStatement(
                    ReturnExpression(
                        Token(TokenKind.Return, Whitespace("\n")),
                        null),
                    Token(TokenKind.Semicolon)),
            ],
            null);
        
        var target =
            ((node.Statements[1] as ExpressionStatementSyntax)!
                .Expression as ReturnExpressionSyntax)!
                    .Return;
        
        target.GetPreviousToken().ShouldBe(returnToken);
    }

    [Fact]
    public void GetFirstAncestorOfType_ReturnsNull_ForNodeWithoutAncestorOfType()
    {
        var node = Root(
            Block(
                [
                    ExpressionStatement(
                        ReturnExpression(
                            Token(TokenKind.Return),
                            null),
                        Token(TokenKind.Semicolon))
                ],
            null),
            Token(TokenKind.EndOfFile));
        
        var target =
            ((node.Block.Statements[0] as ExpressionStatementSyntax)!
                .Expression as ReturnExpressionSyntax)!
                    .Return;
        
        target.GetFirstAncestorOfType<NumberExpressionSyntax>().ShouldBeNull();
    }

    [Fact]
    public void GetFirstAncestorOfType_ReturnsParentToken_ForTokenTrivia()
    {
        var token = Token(TokenKind.Number, "1", UnexpectedToken(TokenKind.Mut));

        var target = (token.LeadingTrivia[0] as UnexpectedTokenTrivia)!;

        target.GetFirstAncestorOfType<Token>().ShouldBe(token);
    }

    [Fact]
    public void GetFirstAncestorOfType_ReturnsFirstAncestor_ForNodeWithAncestor()
    {
        var node = Root(
            Block(
                [
                    ExpressionStatement(
                        ReturnExpression(
                            Token(TokenKind.Return),
                            null),
                        Token(TokenKind.Semicolon))
                ],
            null),
            Token(TokenKind.EndOfFile));
        
        var target =
            ((node.Block.Statements[0] as ExpressionStatementSyntax)!
                .Expression as ReturnExpressionSyntax)!;
        
        target.GetFirstAncestorOfType<RootSyntax>().ShouldBe(node);
    }
}
