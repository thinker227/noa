using Noa.Compiler.Nodes;
using Noa.Compiler.Tests;

namespace Noa.Compiler.Parsing.Tests;

public class RootAndBlockExpressionTests
{
    [Fact]
    public void Parses_Root_WithTrailingExpression()
    {
        var p = ParseAssertion.Create("""
        a();
        b();
        c()
        """, p => p.ParseRoot());
        
        p.Diagnostics.DiagnosticsShouldBe([]);

        p.N<Root>();
        {
            p.N<ExpressionStatement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }
            
            p.N<ExpressionStatement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("b"));
                }
            }
            
            p.N<CallExpression>();
            {
                p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("c"));
            }
        }

        p.End();
    }

    [Fact]
    public void Parses_BlockExpression_WithTrailingExpression()
    {
        var p = ParseAssertion.Create("""
        {
            a();
            b();
            x
        }
        """, p => p.ParseBlockExpression());
        
        p.Diagnostics.DiagnosticsShouldBe([]);

        p.N<BlockExpression>();
        {
            p.N<ExpressionStatement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }
            
            p.N<ExpressionStatement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("b"));
                }
            }
            
            p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("x"));
        }

        p.End();
    }

    [Fact]
    public void Parses_Root_Empty()
    {
        var p = ParseAssertion.Create("", p => p.ParseRoot());

        p.Diagnostics.DiagnosticsShouldBe([]);
        
        p.N<Root>();

        p.End();
    }

    [Fact]
    public void Parses_BlockExpression_Empty()
    {
        var p = ParseAssertion.Create("{}", p => p.ParseBlockExpression());

        p.Diagnostics.DiagnosticsShouldBe([]);
        
        p.N<BlockExpression>();

        p.End();
    }

    [Fact]
    public void Parses_Root_WithSingleStatement()
    {
        var p = ParseAssertion.Create("a();", p => p.ParseRoot());

        p.Diagnostics.DiagnosticsShouldBe([]);
        
        p.N<Root>();
        {
            p.N<ExpressionStatement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }
        }

        p.End();
    }

    [Fact]
    public void Parses_BlockExpression_WithSingleStatement()
    {
        var p = ParseAssertion.Create("""
        {
            a();
        }
        """, p => p.ParseBlockExpression());

        p.Diagnostics.DiagnosticsShouldBe([]);
        
        p.N<BlockExpression>();
        {
            p.N<ExpressionStatement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }
        }

        p.End();
    }

    [Fact]
    public void Parses_BlockExpression_WithOnlyTrailingExpression()
    {
        var p = ParseAssertion.Create("""
        {
            x
        }
        """, p => p.ParseBlockExpression());

        p.Diagnostics.DiagnosticsShouldBe([]);
        
        p.N<BlockExpression>();
        {
            p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("x"));
        }

        p.End();
    }

    [Fact]
    public void Parses_BlockExpression_WithStatement_WithMissingSemicolon_BeforeTrailingExpression()
    {
        var p = ParseAssertion.Create("""
        {
            a()
            x
        }
        """, p => p.ParseBlockExpression());

        p.Diagnostics.DiagnosticsShouldBe([
            (ParseDiagnostics.ExpectedKinds.Id, new("test-input", 14, 15))
        ]);

        p.N<BlockExpression>();
        {
            p.N<ExpressionStatement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }

            p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("x"));
        }

        p.End();
    }

    [Fact]
    public void Parses_Root_WithStatement_WithMissingSemicolon_BeforeStatement()
    {
        var p = ParseAssertion.Create("""
        a()
        b();
        """, p => p.ParseRoot());

        p.Diagnostics.DiagnosticsShouldBe([
            (ParseDiagnostics.ExpectedKinds.Id, new("test-input", 4, 5))
        ]);

        p.N<Root>();
        {
            p.N<ExpressionStatement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }

            p.N<ExpressionStatement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("b"));
                }
            }
        }

        p.End();
    }
    
    [Fact]
    public void Parses_Root_WithStatement_BeforeNonsense()
    {
        var p = ParseAssertion.Create("""
        a();
        mut
        """, p => p.ParseRoot());

        p.Diagnostics.DiagnosticsShouldBe([
            (ParseDiagnostics.UnexpectedToken.Id, new("test-input", 5, 8)),
            (ParseDiagnostics.ExpectedKinds.Id, new("test-input", 5, 8))
        ]);

        p.N<Root>();
        {
            p.N<ExpressionStatement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }
        }

        p.End();
    }
    
    [Fact]
    public void Parses_BlockExpression_WithTrailingExpression_BeforeNonsense()
    {
        var p = ParseAssertion.Create("""
        {
            x
            mut
        }
        """, p => p.ParseBlockExpression());

        p.Diagnostics.DiagnosticsShouldBe([
            (ParseDiagnostics.UnexpectedToken.Id, new("test-input", 12, 15))
        ]);

        p.N<BlockExpression>();
        {
            p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("x"));
        }

        p.End();
    }
    
    [Fact]
    public void Synchronizes_BlockExpression_WithStartOfStatementOrExpression()
    {
        var p = ParseAssertion.Create("""
        {
            a();
            mut
            b();
        }
        """, p => p.ParseBlockExpression());

        p.Diagnostics.DiagnosticsShouldBe([
            (ParseDiagnostics.UnexpectedToken.Id, new("test-input", 15, 18)),
            (ParseDiagnostics.ExpectedKinds.Id, new("test-input", 15, 18))
        ]);

        p.N<BlockExpression>();
        {
            p.N<ExpressionStatement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }
            
            p.N<ExpressionStatement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("b"));
                }
            }
        }

        p.End();
    }
    
    [Fact]
    public void Synchronizes_BlockExpression_WithClosingBrace()
    {
        var p = ParseAssertion.Create("""
        {
            a();
            mut
        }
        """, p => p.ParseBlockExpression());

        p.Diagnostics.DiagnosticsShouldBe([
            (ParseDiagnostics.UnexpectedToken.Id, new("test-input", 15, 18)),
            (ParseDiagnostics.ExpectedKinds.Id, new("test-input", 15, 18))
        ]);

        p.N<BlockExpression>();
        {
            p.N<ExpressionStatement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }
        }

        p.End();
    }
    
    [Fact]
    public void Synchronizes_Root_WithStartOfStatementOrExpression()
    {
        var p = ParseAssertion.Create("""
        a();
        mut
        b();
        """, p => p.ParseRoot());

        p.Diagnostics.DiagnosticsShouldBe([
            (ParseDiagnostics.UnexpectedToken.Id, new("test-input", 5, 8)),
            (ParseDiagnostics.ExpectedKinds.Id, new("test-input", 5, 8))
        ]);

        p.N<Root>();
        {
            p.N<ExpressionStatement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }
            
            p.N<ExpressionStatement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("b"));
                }
            }
        }

        p.End();
    }
    
    [Fact]
    public void Synchronizes_Root_WithEndOfFile()
    {
        var p = ParseAssertion.Create("""
        a();
        mut
        """, p => p.ParseRoot());

        p.Diagnostics.DiagnosticsShouldBe([
            (ParseDiagnostics.UnexpectedToken.Id, new("test-input", 5, 8)),
            (ParseDiagnostics.ExpectedKinds.Id, new("test-input", 5, 8))
        ]);

        p.N<Root>();
        {
            p.N<ExpressionStatement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }
        }

        p.End();
    }

    [Fact]
    public void Parses_LoopExpressions_InBlockExpressions_AsTrailingExpressions()
    {
        var p = ParseAssertion.Create("""
        {
            loop {}
        }
        """, p => p.ParseBlockExpression());

        p.Diagnostics.DiagnosticsShouldBe([]);

        p.N<BlockExpression>();
        {
            p.N<LoopExpression>();
            {
                p.N<BlockExpression>();
            }
        }

        p.End();
    }

    [Fact]
    public void Parses_IfExpressions_InBlockExpressions_AsTrailingExpressions()
    {
        var p = ParseAssertion.Create("""
        {
            if x {} else {}
        }
        """, p => p.ParseBlockExpression());

        p.Diagnostics.DiagnosticsShouldBe([]);

        p.N<BlockExpression>();
        {
            p.N<IfExpression>();
            {
                p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("x"));

                p.N<BlockExpression>();

                p.N<BlockExpression>();
            }
        }

        p.End();
    }

    [Fact]
    public void Parses_BlockExpressions_InBlockExpressions_AsTrailingExpressions()
    {
        var p = ParseAssertion.Create("""
        {
            {}
        }
        """, p => p.ParseBlockExpression());

        p.Diagnostics.DiagnosticsShouldBe([]);

        p.N<BlockExpression>();
        {
            p.N<BlockExpression>();
        }

        p.End();
    }

    [Fact]
    public void Parses_LoopExpressions_WithoutSemicolon_InBlockExpressions_AsStatements()
    {
        var p = ParseAssertion.Create("""
        {
            loop {}
            0
        }
        """, p => p.ParseBlockExpression());

        p.Diagnostics.DiagnosticsShouldBe([]);

        p.N<BlockExpression>();
        {
            p.N<ExpressionStatement>();
            {
                p.N<LoopExpression>();
                {
                    p.N<BlockExpression>();
                }
            }

            p.N<NumberExpression>(n => n.Value.ShouldBe(0));
        }

        p.End();
    }

    [Fact]
    public void Parses_IfExpressions_WithoutSemicolon_InBlockExpressions_AsStatements()
    {
        var p = ParseAssertion.Create("""
        {
            if x {} else {}
            0
        }
        """, p => p.ParseBlockExpression());

        p.Diagnostics.DiagnosticsShouldBe([]);

        p.N<BlockExpression>();
        {
            p.N<ExpressionStatement>();
            {
                p.N<IfExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("x"));

                    p.N<BlockExpression>();

                    p.N<BlockExpression>();
                }
            }

            p.N<NumberExpression>(n => n.Value.ShouldBe(0));
        }

        p.End();
    }

    [Fact]
    public void Parses_BlockExpressions_WithoutSemicolon_InBlockExpressions_AsStatements()
    {
        var p = ParseAssertion.Create("""
        {
            {}
            0
        }
        """, p => p.ParseBlockExpression());

        p.Diagnostics.DiagnosticsShouldBe([]);

        p.N<BlockExpression>();
        {
            p.N<ExpressionStatement>();
            {
                p.N<BlockExpression>();
            }

            p.N<NumberExpression>(n => n.Value.ShouldBe(0));
        }

        p.End();
    }

    [Fact]
    public void Parses_LoopExpressions_WithSemicolon_InBlockExpressions_AsStatements_AndErrors()
    {
        var p = ParseAssertion.Create("""
        {
            loop {};
            0
        }
        """, p => p.ParseBlockExpression());

        p.Diagnostics.DiagnosticsShouldBe([
            (ParseDiagnostics.UnexpectedToken.Id, new("test-input", 13, 14)),
            (ParseDiagnostics.ExpectedKinds.Id, new("test-input", 13, 14))
        ]);

        p.N<BlockExpression>();
        {
            p.N<ExpressionStatement>();
            {
                p.N<LoopExpression>();
                {
                    p.N<BlockExpression>();
                }
            }

            p.N<NumberExpression>(n => n.Value.ShouldBe(0));
        }

        p.End();
    }

    [Fact]
    public void Parses_IfExpressions_WithSemicolon_InBlockExpressions_AsStatements_AndErrors()
    {
        var p = ParseAssertion.Create("""
        {
            if x {} else {};
            0
        }
        """, p => p.ParseBlockExpression());

        p.Diagnostics.DiagnosticsShouldBe([
            (ParseDiagnostics.UnexpectedToken.Id, new("test-input", 21, 22)),
            (ParseDiagnostics.ExpectedKinds.Id, new("test-input", 21, 22))
        ]);

        p.N<BlockExpression>();
        {
            p.N<ExpressionStatement>();
            {
                p.N<IfExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("x"));

                    p.N<BlockExpression>();

                    p.N<BlockExpression>();
                }
            }

            p.N<NumberExpression>(n => n.Value.ShouldBe(0));
        }

        p.End();
    }

    [Fact]
    public void Parses_BlockExpressions_WithSemicolon_InBlockExpressions_AsStatements_AndErrors()
    {
        var p = ParseAssertion.Create("""
        {
            {};
            0
        }
        """, p => p.ParseBlockExpression());

        p.Diagnostics.DiagnosticsShouldBe([
            (ParseDiagnostics.UnexpectedToken.Id, new("test-input", 8, 9)),
            (ParseDiagnostics.ExpectedKinds.Id, new("test-input", 8, 9))
        ]);

        p.N<BlockExpression>();
        {
            p.N<ExpressionStatement>();
            {
                p.N<BlockExpression>();
            }

            p.N<NumberExpression>(n => n.Value.ShouldBe(0));
        }

        p.End();
    }
}
