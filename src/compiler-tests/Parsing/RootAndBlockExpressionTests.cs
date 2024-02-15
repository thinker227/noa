using Noa.Compiler.Nodes;
using Noa.Compiler.Tests;

namespace Noa.Compiler.Parsing.Tests;

public class RootAndBlockExpressionTests
{
    [Fact]
    public void DoesNotParse_Root_WithTrailingExpression()
    {
        using var p = ParseAssertion.Create("""
        a();
        b();
        c()
        """, p => p.ParseRoot());
        
        p.Diagnostics.DiagnosticsShouldBe([
            (ParseDiagnostics.ExpectedKinds.Id, new("test-input", 13, 13))
        ]);

        p.N<Root>();
        {
            p.N<Statement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }
            
            p.N<Statement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("b"));
                }
            }
            
            p.N<Statement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("c"));
                }
            }
        }
    }

    [Fact]
    public void Parses_BlockExpression_WithTrailingExpression()
    {
        using var p = ParseAssertion.Create("""
        {
            a();
            b();
            x
        }
        """, p => p.ParseBlockExpression());
        
        p.Diagnostics.DiagnosticsShouldBe([]);

        p.N<BlockExpression>();
        {
            p.N<Statement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }
            
            p.N<Statement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("b"));
                }
            }
            
            p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("x"));
        }
    }

    [Fact]
    public void Parses_Root_Empty()
    {
        using var p = ParseAssertion.Create("", p => p.ParseRoot());

        p.Diagnostics.DiagnosticsShouldBe([]);
        
        p.N<Root>();
    }

    [Fact]
    public void Parses_BlockExpression_Empty()
    {
        using var p = ParseAssertion.Create("{}", p => p.ParseBlockExpression());

        p.Diagnostics.DiagnosticsShouldBe([]);
        
        p.N<BlockExpression>();
    }

    [Fact]
    public void Parses_Root_WithSingleStatement()
    {
        using var p = ParseAssertion.Create("a();", p => p.ParseRoot());

        p.Diagnostics.DiagnosticsShouldBe([]);
        
        p.N<Root>();
        {
            p.N<Statement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }
        }
    }

    [Fact]
    public void Parses_BlockExpression_WithSingleStatement()
    {
        using var p = ParseAssertion.Create("""
        {
            a();
        }
        """, p => p.ParseBlockExpression());

        p.Diagnostics.DiagnosticsShouldBe([]);
        
        p.N<BlockExpression>();
        {
            p.N<Statement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }
        }
    }

    [Fact]
    public void Parses_BlockExpression_WithOnlyTrailingExpression()
    {
        using var p = ParseAssertion.Create("""
        {
            x
        }
        """, p => p.ParseBlockExpression());

        p.Diagnostics.DiagnosticsShouldBe([]);
        
        p.N<BlockExpression>();
        {
            p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("x"));
        }
    }

    [Fact]
    public void Parses_BlockExpression_WithStatement_WithMissingSemicolon_BeforeTrailingExpression()
    {
        using var p = ParseAssertion.Create("""
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
            p.N<Statement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }

            p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("x"));
        }
    }

    [Fact]
    public void Parses_Root_WithStatement_WithMissingSemicolon_BeforeStatement()
    {
        using var p = ParseAssertion.Create("""
        a()
        b();
        """, p => p.ParseRoot());

        p.Diagnostics.DiagnosticsShouldBe([
            (ParseDiagnostics.ExpectedKinds.Id, new("test-input", 4, 5))
        ]);

        p.N<Root>();
        {
            p.N<Statement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }

            p.N<Statement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("b"));
                }
            }
        }
    }
    
    [Fact]
    public void Parses_Root_WithStatement_BeforeNonsense()
    {
        using var p = ParseAssertion.Create("""
        a();
        mut
        """, p => p.ParseRoot());

        p.Diagnostics.DiagnosticsShouldBe([
            (ParseDiagnostics.UnexpectedToken.Id, new("test-input", 5, 8)),
            (ParseDiagnostics.ExpectedKinds.Id, new("test-input", 5, 8))
        ]);

        p.N<Root>();
        {
            p.N<Statement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }
        }
    }
    
    [Fact]
    public void Parses_BlockExpression_WithTrailingExpression_BeforeNonsense()
    {
        using var p = ParseAssertion.Create("""
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
    }
    
    [Fact]
    public void Synchronizes_BlockExpression_WithStartOfStatementOrExpression()
    {
        using var p = ParseAssertion.Create("""
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
            p.N<Statement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }
            
            p.N<Statement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("b"));
                }
            }
        }
    }
    
    [Fact]
    public void Synchronizes_BlockExpression_WithClosingBrace()
    {
        using var p = ParseAssertion.Create("""
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
            p.N<Statement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }
        }
    }
    
    [Fact]
    public void Synchronizes_Root_WithStartOfStatementOrExpression()
    {
        using var p = ParseAssertion.Create("""
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
            p.N<Statement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }
            
            p.N<Statement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("b"));
                }
            }
        }
    }
    
    [Fact]
    public void Synchronizes_Root_WithEndOfFile()
    {
        using var p = ParseAssertion.Create("""
        a();
        mut
        """, p => p.ParseRoot());

        p.Diagnostics.DiagnosticsShouldBe([
            (ParseDiagnostics.UnexpectedToken.Id, new("test-input", 5, 8)),
            (ParseDiagnostics.ExpectedKinds.Id, new("test-input", 5, 8))
        ]);

        p.N<Root>();
        {
            p.N<Statement>();
            {
                p.N<CallExpression>();
                {
                    p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
                }
            }
        }
    }
}
