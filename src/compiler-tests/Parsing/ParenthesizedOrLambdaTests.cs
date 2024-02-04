using Noa.Compiler.Nodes;

namespace Noa.Compiler.Parsing.Tests;

public class ParenthesizedOrLambdaTests
{
    [Fact]
    public void Parses_MultipleNameList_WithArrow_AsLambda()
    {
        var p = ParseAssertion.Create("""
        (a, b, c) => 0
        """, p => p.ParseParenthesizedOrLambdaExpression());

        p.N<LambdaExpression>();
        {
            p.N<Parameter>();
            {
                p.N<Identifier>(i => i.Name.ShouldBe("a"));
            }

            p.N<Parameter>();
            {
                p.N<Identifier>(i => i.Name.ShouldBe("b"));
            }

            p.N<Parameter>();
            {
                p.N<Identifier>(i => i.Name.ShouldBe("c"));
            }

            p.N<NumberExpression>(n => n.Value.ShouldBe(0));
        }
    }

    [Fact]
    public void Parses_MultipleNameList_WithoutArrow_AsTuple()
    {
        var p = ParseAssertion.Create("""
        (a, b, c)
        """, p => p.ParseParenthesizedOrLambdaExpression());

        p.N<TupleExpression>();
        {
            p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));

            p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("b"));

            p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("c"));
        }
    }

    [Fact]
    public void Parses_SingleNameList_WithArrow_AsLambda()
    {
        var p = ParseAssertion.Create("""
        (x) => 0
        """, p => p.ParseParenthesizedOrLambdaExpression());

        p.N<LambdaExpression>();
        {
            p.N<Parameter>();
            {
                p.N<Identifier>(i => i.Name.ShouldBe("x"));
            }

            p.N<NumberExpression>(n => n.Value.ShouldBe(0));
        }
    }

    [Fact]
    public void Parses_SingleNameList_WithoutArrow_AsParenthesized()
    {
        var p = ParseAssertion.Create("""
        (x)
        """, p => p.ParseParenthesizedOrLambdaExpression());

        p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("x"));
    }
}
