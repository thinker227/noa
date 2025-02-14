using Noa.Compiler.Syntax.Green;
using Noa.Compiler.Tests;
using TokenKind = Noa.Compiler.Syntax.TokenKind;

namespace Noa.Compiler.Parsing.Tests;

public sealed class IfExpressionTests
{
    [Fact]
    public void Else_IsRequired_InExpressions()
    {
        var p = ParseAssertion.Create(
            "if true {}",
            p => p.ParseExpressionOrError());

        p.N<IfExpressionSyntax>();
        p.T(TokenKind.If);
        p.D(ParseDiagnostics.ElseOmitted.Id);
        {
            p.N<BoolExpressionSyntax>();
            {
                p.T(TokenKind.True);
            }

            p.N<BlockExpressionSyntax>();
            {
                p.T(TokenKind.OpenBrace);
                
                p.N<BlockSyntax>();
                {
                    p.N<SyntaxList<StatementSyntax>>();
                }

                p.T(TokenKind.CloseBrace);
            }

            p.N<ElseClauseSyntax>();
            {
                p.E(); // else
                
                p.N<BlockExpressionSyntax>();
                {
                    p.E(); // {

                    p.N<BlockSyntax>();
                    {
                        p.N<SyntaxList<StatementSyntax>>();
                    }

                    p.E(); // }
                }
            }
        }

        p.End();
    }

    [Fact]
    public void Else_IsNotRequired_InStatements()
    {
        var p = ParseAssertion.Create(
            "if true {}",
            p => p.ParseRoot());

        p.N<RootSyntax>();
        {
            p.N<BlockSyntax>();
            {
                p.N<SyntaxList<StatementSyntax>>();
                {
                    p.N<FlowControlStatementSyntax>();
                    {
                        p.N<IfExpressionSyntax>();
                        p.T(TokenKind.If);
                        {
                            p.N<BoolExpressionSyntax>();
                            {
                                p.T(TokenKind.True);
                            }

                            p.N<BlockExpressionSyntax>();
                            {
                                p.T(TokenKind.OpenBrace);

                                p.N<BlockSyntax>();
                                {
                                    p.N<SyntaxList<StatementSyntax>>();
                                }

                                p.T(TokenKind.CloseBrace);
                            }
                        }
                    }
                }
            }
        }

        p.T(TokenKind.EndOfFile);

        p.End();
    }
    
    [Fact]
    public void Else_IsAllowed_InExpressions()
    {
        var p = ParseAssertion.Create(
            "if true {} else {}",
            p => p.ParseExpressionOrError());
        
        p.N<IfExpressionSyntax>();
        p.T(TokenKind.If);
        {
            p.N<BoolExpressionSyntax>();
            {
                p.T(TokenKind.True);
            }

            p.N<BlockExpressionSyntax>();
            {
                p.T(TokenKind.OpenBrace);

                p.N<BlockSyntax>();
                {
                    p.N<SyntaxList<StatementSyntax>>();
                }

                p.T(TokenKind.CloseBrace);
            }

            p.N<ElseClauseSyntax>();
            p.T(TokenKind.Else);
            {
                p.N<BlockExpressionSyntax>();
                {
                    p.T(TokenKind.OpenBrace);

                    p.N<BlockSyntax>();
                    {
                        p.N<SyntaxList<StatementSyntax>>();
                    }

                    p.T(TokenKind.CloseBrace);
                }
            }
        }

        p.End();
    }

    [Fact]
    public void IfWithoutElse_InBlock_ParsesAsStatement()
    {
        var p = ParseAssertion.Create(
            "if true {}",
            p => p.ParseRoot());

        p.N<RootSyntax>();
        {
            p.N<BlockSyntax>();
            {
                p.N<SyntaxList<StatementSyntax>>();
                {
                    p.N<FlowControlStatementSyntax>();
                    {
                        p.N<IfExpressionSyntax>();
                        p.T(TokenKind.If);
                        {
                            p.N<BoolExpressionSyntax>();
                            {
                                p.T(TokenKind.True);                            
                            }

                            p.N<BlockExpressionSyntax>();
                            {
                                p.T(TokenKind.OpenBrace);

                                p.N<BlockSyntax>();
                                {
                                    p.N<SyntaxList<StatementSyntax>>();
                                }

                                p.T(TokenKind.CloseBrace);
                            }
                        }
                    }
                }
            }
        }

        p.T(TokenKind.EndOfFile);

        p.End();
    }
}
