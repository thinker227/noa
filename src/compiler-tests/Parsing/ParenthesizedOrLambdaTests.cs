#if false
using Noa.Compiler.Syntax.Green;
using TokenKind = Noa.Compiler.Syntax.TokenKind;

namespace Noa.Compiler.Parsing.Tests;

public class ParenthesizedOrLambdaTests
{
    [Fact]
    public void Parses_EmptyList_WithArrow_AsLambda()
    {
        var p = ParseAssertion.Create(
            "() => 0",
            p => p.ParseParenthesizedOrLambdaExpression());

        p.N<LambdaExpressionSyntax>();
        {
            p.N<NumberExpressionSyntax>(n => n.Value.ShouldBe(0));
        }

        p.End();
    }

    [Fact]
    public void Parses_EmptyList_WithoutArrow_AsNil()
    {
        var p = ParseAssertion.Create(
            "()",
            p => p.ParseParenthesizedOrLambdaExpression());

        p.N<NilExpressionSyntax>();

        p.End();
    }
    
    [Fact]
    public void Parses_MultipleNameList_WithArrow_AsLambda()
    {
        var p = ParseAssertion.Create(
            "(a, b, c) => 0",
            p => p.ParseParenthesizedOrLambdaExpression());

        p.N<LambdaExpressionSyntax>();
        {
            p.N<ParameterSyntax>();
            {
                p.N<IdentifierSyntax>(i => i.Name.ShouldBe("a"));
            }

            p.N<ParameterSyntax>();
            {
                p.N<IdentifierSyntax>(i => i.Name.ShouldBe("b"));
            }

            p.N<ParameterSyntax>();
            {
                p.N<IdentifierSyntax>(i => i.Name.ShouldBe("c"));
            }

            p.N<NumberExpressionSyntax>(n => n.Value.ShouldBe(0));
        }

        p.End();
    }

    [Fact]
    public void Parses_MultipleNameList_WithoutArrow_AsTuple()
    {
        var p = ParseAssertion.Create(
            "(a, b, c)",
            p => p.ParseParenthesizedOrLambdaExpression());

        p.N<TupleExpressionSyntax>();
        {
            p.N<IdentifierExpressionSyntax>();
            {
                p.T(TokenKind.Name, t => t.Text.ShouldBe("a"));
            }

            p.N<IdentifierExpressionSyntax>();
            {
                p.T(TokenKind.Name, t => t.Text.ShouldBe("b"));
            }

            p.N<IdentifierExpressionSyntax>();
            {
                p.T(TokenKind.Name, t => t.Text.ShouldBe("c"));
            }
        }

        p.End();
    }

    [Fact]
    public void Parses_SingleNameList_WithArrow_AsLambda()
    {
        var p = ParseAssertion.Create(
            "(x) => 0",
            p => p.ParseParenthesizedOrLambdaExpression());

        p.N<LambdaExpressionSyntax>();
        {
            p.N<ParameterSyntax>();
            {
                p.N<IdentifierSyntax>(i => i.Name.ShouldBe("x"));
            }

            p.N<NumberExpressionSyntax>(n => n.Value.ShouldBe(0));
        }

        p.End();
    }

    [Fact]
    public void Parses_SingleNameList_WithoutArrow_AsParenthesized()
    {
        var p = ParseAssertion.Create(
            "(x)",
            p => p.ParseParenthesizedOrLambdaExpression());

        p.N<IdentifierExpressionSyntax>();
        {
            p.T(TokenKind.Name, t => t.Text.ShouldBe("x"));
        }

        p.End();
    }
    
    [Fact]
    public void Parses_SingleCallExpression_WithoutArrow_AsParenthesized()
    {
        var p = ParseAssertion.Create(
            "(f())",
            p => p.ParseParenthesizedOrLambdaExpression());

        p.N<CallExpressionSyntax>();
        {
            p.N<IdentifierExpressionSyntax>();
            {
                p.T(TokenKind.Name, t => t.Text.ShouldBe("f"));
            }
        }

        p.End();
    }
    
    [Fact]
    public void Parses_SingleBinaryExpression_WithoutArrow_AsParenthesized()
    {
        var p = ParseAssertion.Create(
            "(a + b)",
            p => p.ParseParenthesizedOrLambdaExpression());

        p.N<BinaryExpressionSyntax>(b => b.Kind.ShouldBe(BinaryKind.Plus));
        {
            p.N<IdentifierExpressionSyntax>();
            {
                p.T(TokenKind.Name, t => t.Text.ShouldBe("a"));
            }
            
            p.N<IdentifierExpressionSyntax>();
            {
                p.T(TokenKind.Name, t => t.Text.ShouldBe("b"));
            }
        }

        p.End();
    }

    [Fact]
    public void Parses_MutName_ThenExpression_WithArrow_AsLambda()
    {
        var p = ParseAssertion.Create(
            "(mut x, 0) => 1",
            p => p.ParseParenthesizedOrLambdaExpression());

        p.N<LambdaExpressionSyntax>();
        {
            p.N<ParameterSyntax>(param => param.IsMutable.ShouldBeTrue());
            {
                p.N<IdentifierSyntax>(i => i.Name.ShouldBe("x"));
            }
            
            // 0 should not be parsed as anything.

            p.N<NumberExpressionSyntax>(n => n.Value.ShouldBe(1));
        }

        p.End();
    }

    [Fact]
    public void Parses_MutName_WithoutArrow_AsLambda()
    {
        var p = ParseAssertion.Create(
            "(mut x)",
            p => p.ParseParenthesizedOrLambdaExpression());

        p.N<LambdaExpressionSyntax>();
        {
            p.N<ParameterSyntax>(param => param.IsMutable.ShouldBeTrue());
            {
                p.N<IdentifierSyntax>(i => i.Name.ShouldBe("x"));
            }

            // The body expression should be an error.
            p.N<ErrorExpressionSyntax>();
        }

        p.End();
    }

    [Fact]
    public void Parses_Expression_ThenMutName_WithoutArrow_AsTuple()
    {
        var p = ParseAssertion.Create(
            "(0, mut x)",
            p => p.ParseParenthesizedOrLambdaExpression());

        p.N<TupleExpressionSyntax>();
        {
            p.N<NumberExpressionSyntax>(n => n.Value.ShouldBe(0));
            
            // mut should not be parsed as anything.

            p.N<IdentifierExpressionSyntax>();
            {
                p.T(TokenKind.Name, t => t.Text.ShouldBe("x"));
            }
        }

        p.End();
    }
}
#endif
