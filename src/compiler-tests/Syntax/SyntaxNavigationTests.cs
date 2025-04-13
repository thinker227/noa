namespace Noa.Compiler.Syntax.Tests;

public class SyntaxNavigationTests
{
    [Fact]
    public void GetFirstToken_ReturnsSelf_ForToken()
    {
        var token = (Token)new Green.Token(TokenKind.True, null, "", 4).ToRed(0, null!);

        token.GetFirstToken().ShouldBeEquivalentTo(token);
    }

    [Fact]
    public void GetFirstToken_GetsFirstToken_InNode()
    {
        var node = (LetDeclarationSyntax)new Green.LetDeclarationSyntax()
        {
            Let = new Green.Token(TokenKind.Let, null, "", 3),
            Mut = null,
            Name = new Green.Token(TokenKind.Name, "x", "", 1),
            EqualsToken = new Green.Token(TokenKind.Equals, null, "", 1),
            Value = new Green.NumberExpressionSyntax()
            {
                Value = new Green.Token(TokenKind.Number, "1", "", 1)
            },
            Semicolon = new Green.Token(TokenKind.Semicolon, null, "", 1)
        }.ToRed(0, null!);

        node.GetFirstToken().ShouldBeEquivalentTo(node.Let);
    }

    [Fact]
    public void GetFirstToken_GetsFirstToken_InChildren()
    {
        var node = (RootSyntax)new Green.RootSyntax()
        {
            Block = new Green.BlockSyntax()
            {
                Statements = new Green.SyntaxList<Green.StatementSyntax>([]),
                TrailingExpression = new Green.NumberExpressionSyntax()
                {
                    Value = new Green.Token(TokenKind.Number, "0", "", 1)
                }
            },
            EndOfFile = new Green.Token(TokenKind.EndOfFile, null, "", 0)
        }.ToRed(0, null!);

        node.GetFirstToken().ShouldBeEquivalentTo(
            ((NumberExpressionSyntax)node.Block.TrailingExpression!).Value);
    }

    [Fact]
    public void GetLastToken_ReturnsSelf_ForToken()
    {
        var token = (Token)new Green.Token(TokenKind.True, null, "", 4).ToRed(0, null!);

        token.GetLastToken().ShouldBeEquivalentTo(token);
    }

    [Fact]
    public void GetLastToken_GetsLastToken_InNode()
    {
        var node = (LetDeclarationSyntax)new Green.LetDeclarationSyntax()
        {
            Let = new Green.Token(TokenKind.Let, null, "", 3),
            Mut = null,
            Name = new Green.Token(TokenKind.Name, "x", "", 1),
            EqualsToken = new Green.Token(TokenKind.Equals, null, "", 1),
            Value = new Green.NumberExpressionSyntax()
            {
                Value = new Green.Token(TokenKind.Number, "1", "", 1)
            },
            Semicolon = new Green.Token(TokenKind.Semicolon, null, "", 1)
        }.ToRed(0, null!);

        node.GetLastToken().ShouldBeEquivalentTo(node.Semicolon);
    }

    [Fact]
    public void GetLastToken_GetsLastToken_InChildren()
    {
        var node = (BlockSyntax)new Green.BlockSyntax()
        {
            Statements = new Green.SyntaxList<Green.StatementSyntax>(
            [
                new Green.ExpressionStatementSyntax()
                {
                    Expression = new Green.NumberExpressionSyntax()
                    {
                        Value = new Green.Token(TokenKind.Number, "0", "", 1)
                    },
                    Semicolon = new Green.Token(TokenKind.Semicolon, null, "", 1)
                },
                new Green.ExpressionStatementSyntax()
                {
                    Expression = new Green.NumberExpressionSyntax()
                    {
                        Value = new Green.Token(TokenKind.Number, "1", "", 1)
                    },
                    Semicolon = new Green.Token(TokenKind.Semicolon, null, "", 1)
                }
            ]),
            TrailingExpression = null
        }.ToRed(0, null!);

        node.GetLastToken().ShouldBeEquivalentTo(
            ((ExpressionStatementSyntax)node.Statements[1]).Semicolon);
    }
}
