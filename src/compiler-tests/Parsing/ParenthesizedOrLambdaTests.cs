using Noa.Compiler.Nodes;

namespace Noa.Compiler.Parsing.Tests;

public class ParenthesizedOrLambdaTests
{
    [Fact]
    public void Parses_EmptyList_WithArrow_AsLambda()
    {
        using var p = ParseAssertion.Create(
            "() => 0",
            p => p.ParseParenthesizedOrLambdaExpression());

        p.N<LambdaExpression>();
        {
            p.N<NumberExpression>(n => n.Value.ShouldBe(0));
        }
    }

    [Fact]
    public void Parses_EmptyList_WithoutArrow_AsError()
    {
        using var p = ParseAssertion.Create(
            "()",
            p => p.ParseParenthesizedOrLambdaExpression());

        p.N<ErrorExpression>();
    }
    
    [Fact]
    public void Parses_MultipleNameList_WithArrow_AsLambda()
    {
        using var p = ParseAssertion.Create(
            "(a, b, c) => 0",
            p => p.ParseParenthesizedOrLambdaExpression());

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
        using var p = ParseAssertion.Create(
            "(a, b, c)",
            p => p.ParseParenthesizedOrLambdaExpression());

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
        using var p = ParseAssertion.Create(
            "(x) => 0",
            p => p.ParseParenthesizedOrLambdaExpression());

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
        using var p = ParseAssertion.Create(
            "(x)",
            p => p.ParseParenthesizedOrLambdaExpression());

        p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("x"));
    }
    
    [Fact]
    public void Parses_SingleCallExpression_WithoutArrow_AsParenthesized()
    {
        using var p = ParseAssertion.Create(
            "(f())",
            p => p.ParseParenthesizedOrLambdaExpression());

        p.N<CallExpression>();
        {
            p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("f"));
        }
    }
    
    [Fact]
    public void Parses_SingleBinaryExpression_WithoutArrow_AsParenthesized()
    {
        using var p = ParseAssertion.Create(
            "(a + b)",
            p => p.ParseParenthesizedOrLambdaExpression());

        p.N<BinaryExpression>(b => b.Kind.ShouldBe(BinaryKind.Plus));
        {
            p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("a"));
            
            p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("b"));
        }
    }

    [Fact]
    public void Parses_MutName_ThenExpression_WithArrow_AsLambda()
    {
        using var p = ParseAssertion.Create(
            "(mut x, 0) => 1",
            p => p.ParseParenthesizedOrLambdaExpression());

        p.N<LambdaExpression>();
        {
            p.N<Parameter>(param => param.IsMutable.ShouldBeTrue());
            {
                p.N<Identifier>(i => i.Name.ShouldBe("x"));
            }
            
            // 0 should not be parsed as anything.

            p.N<NumberExpression>(n => n.Value.ShouldBe(1));
        }
    }

    [Fact]
    public void Parses_Expression_ThenMutName_WithoutArrow_AsTuple()
    {
        using var p = ParseAssertion.Create(
            "(0, mut x)",
            p => p.ParseParenthesizedOrLambdaExpression());

        p.N<TupleExpression>();
        {
            p.N<NumberExpression>(n => n.Value.ShouldBe(0));
            
            // mut should not be parsed as anything.

            p.N<IdentifierExpression>(i => i.Identifier.ShouldBe("x"));
        }
    }
}
