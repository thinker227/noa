using Noa.Compiler.Nodes;
using Noa.Compiler.Tests;

namespace Noa.Compiler.Parsing.Tests;

public class StatementTests
{
    [Fact]
    public void Parses_AssignmentStatement_WithIdentifier()
    {
        var p = ParseAssertion.Create("x = 0;", p => p.ParseRoot());
        
        p.Diagnostics.DiagnosticsShouldBe([]);

        p.N<Root>();
        {
            p.N<AssignmentStatement>();
            {
                p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("x"));

                p.N<NumberExpression>(n => n.Value.ShouldBe(0));
            }
        }

        p.End();
    }

    [Fact]
    public void Parses_AssignmentStatement_WithNumber_AndProduces_InvalidLValue()
    {
        var p = ParseAssertion.Create("0 = 1;", p => p.ParseRoot());
        
        p.Diagnostics.DiagnosticsShouldBe([
            (ParseDiagnostics.InvalidLValue.Id, new("test-input", 0, 1))
        ]);

        p.N<Root>();
        {
            p.N<AssignmentStatement>();
            {
                p.N<NumberExpression>(n => n.Value.ShouldBe(0));

                p.N<NumberExpression>(n => n.Value.ShouldBe(1));
            }
        }

        p.End();
    }
}
