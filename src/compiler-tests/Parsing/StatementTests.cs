using Noa.Compiler.Syntax.Green;
using TokenKind = Noa.Compiler.Syntax.TokenKind;

namespace Noa.Compiler.Parsing.Tests;

public class StatementTests
{
    [Fact]
    public void Parses_AssignmentStatement_WithIdentifier()
    {
        var p = ParseAssertion.Create("x = 0;", p => p.ParseRoot());

        p.N<RootSyntax>();
        {
            p.N<BlockSyntax>();
            {
                p.N<SyntaxList<StatementSyntax>>();
                {
                    p.N<AssignmentStatementSyntax>();
                    {
                        p.N<IdentifierExpressionSyntax>();
                        {
                            p.T(TokenKind.Name, t => t.Text.ShouldBe("x"));
                        }

                        p.T(TokenKind.Equals);

                        p.N<NumberExpressionSyntax>();
                        {
                            p.T(TokenKind.Number, t => t.Text.ShouldBe("0"));
                        }

                        p.T(TokenKind.Semicolon);
                    }
                }
            }
        }

        p.T(TokenKind.EndOfFile);

        p.End();
    }

    [Fact]
    public void Parses_AssignmentStatement_WithNumber_AndProduces_InvalidLValue()
    {
        var p = ParseAssertion.Create("0 = 1;", p => p.ParseRoot());

        p.N<RootSyntax>();
        {
            p.N<BlockSyntax>();
            {
                p.N<SyntaxList<StatementSyntax>>();
                {
                    p.N<AssignmentStatementSyntax>();
                    {
                        p.N<NumberExpressionSyntax>();
                        p.D(ParseDiagnostics.InvalidLValue.Id);
                        {
                            p.T(TokenKind.Number, t => t.Text.ShouldBe("0"));
                        }

                        p.T(TokenKind.Equals);

                        p.N<NumberExpressionSyntax>();
                        {
                            p.T(TokenKind.Number, t => t.Text.ShouldBe("1"));
                        }

                        p.T(TokenKind.Semicolon);
                    }
                }
            }
        }

        p.T(TokenKind.EndOfFile);

        p.End();
    }
}
