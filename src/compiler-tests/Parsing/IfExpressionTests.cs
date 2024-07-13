using Noa.Compiler.Nodes;
using Noa.Compiler.Tests;

namespace Noa.Compiler.Parsing.Tests;

public sealed class IfExpressionTests
{
    [Fact]
    public void Else_IsRequired_InExpressions()
    {
        var p = ParseAssertion.Create(
            "if true {}",
            p => p.ParseExpressionOrError());
        
        p.Diagnostics.DiagnosticsShouldBe([
            (ParseDiagnostics.ElseOmitted.Id, new Location("test-input", 0, 2))
        ], ignoreAdditional: true);

        p.N<IfExpression>();
        {
            p.N<BoolExpression>();

            p.N<BlockExpression>();

            p.N<ElseClause>();
            {
                p.N<BlockExpression>();
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

        p.N<Root>();
        {
            p.N<IfExpression>();
            {
                p.N<BoolExpression>();

                p.N<BlockExpression>();
            }
        }

        p.End();
    }
    
    [Fact]
    public void Else_IsAllowed_InExpressions()
    {
        var p = ParseAssertion.Create(
            "if true {} else {}",
            p => p.ParseExpressionOrError());
        
        p.N<IfExpression>();
        {
            p.N<BoolExpression>();

            p.N<BlockExpression>();

            p.N<ElseClause>();
            {
                p.N<BlockExpression>();
            }
        }
    }
}
