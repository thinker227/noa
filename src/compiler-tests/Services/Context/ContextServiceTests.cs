using Noa.Compiler.Symbols;

namespace Noa.Compiler.Services.Context.Tests;

public class ContextServiceTests
{
    private static void Test(
        string testSource,
        SyntaxContextKind expectedContextKind) =>
        Test(testSource, expectedContextKind, null);

    private static void Test(
        string testSource,
        HashSet<Func<ISymbol, bool>> symbolAssertions) =>
        Test(testSource, null, symbolAssertions);

    private static void Test(
        string testSource,
        SyntaxContextKind? expectedContextKind,
        HashSet<Func<ISymbol, bool>>? symbolAssertions)
    {
        var position = testSource.IndexOf('|');
        if (position == -1)
            throw new InvalidOperationException($"Test source does not contain a position marker.");

        var actualSource = $"{testSource[..position]}{testSource[(position + 1)..]}";
        var ast = Ast.Create(new Source(actualSource, "test-source"));

        var ctx = ContextService.GetSyntaxContext(ast, position);

        if (expectedContextKind is not null)
            ctx.Kind.ShouldBe(expectedContextKind.Value);

        if (symbolAssertions is not null)
        {
            var symbols = ctx.AccessibleSymbols.ToHashSet();
            symbols.Count.ShouldBe(symbolAssertions.Count);

            foreach (var symbol in symbols)
            {
                foreach (var assertion in symbolAssertions)
                {
                    if (assertion(symbol))
                    {
                        symbolAssertions.Remove(assertion);
                        goto next;
                    }
                }

                Assert.Fail($"Symbol {symbol} did not match any assertion.");

                next:
                ;
            }
        }
    }

    [Fact]
    public void StatementOrExpressionAfterOpenBrace() => Test(
        "{|}",
        SyntaxContextKind.Statement | SyntaxContextKind.Expression);

    [Fact]
    public void StatementAfterOpenBraceBeforeStatement() => Test(
        "{ | let x = 0; }",
        SyntaxContextKind.Statement);

    [Fact]
    public void VariableNotAvailableBeforeDeclaration() => Test(
        "{ | let x = 0; }",
        []);

    [Fact]
    public void VariableNotAvailableInOwnValue() => Test(
        "let x = |;",
        []);

    [Fact]
    public void VariableAvailableAfterDeclaration() => Test(
        "let x = 0; |",
        [s => s is VariableSymbol { Name: "x" }]);

    [Fact]
    public void ParametersAreAvailableInFunctionBody() => Test(
        "func f(x) => |;",
        [
            s => s is ParameterSymbol { Name: "x" },
            s => s is NomialFunction { Name: "f" }
        ]);

    [Fact]
    public void ParametersAreAvailableInLambdaBody() => Test(
        "(x) => |;",
        [s => s is ParameterSymbol { Name: "x" }]);

    [Fact]
    public void PostIfBodyWithoutElseBeforeBrace() => Test(
        "{ if true {} | }",
        SyntaxContextKind.Expression | SyntaxContextKind.Statement | SyntaxContextKind.PostIfBodyWithoutElse);

    [Fact]
    public void PostIfBodyWithElseBeforeBrace() => Test(
        "{ if true {} else {} | }",
        SyntaxContextKind.Expression | SyntaxContextKind.PostExpression | SyntaxContextKind.Statement);

    [Fact]
    public void PostIfBodyWithoutElseBeforeStatement() => Test(
        "{ if true {} | let x = 0; }",
        SyntaxContextKind.Statement | SyntaxContextKind.PostIfBodyWithoutElse);

    [Fact]
    public void PostIfBodyWithElseBeforeStatement() => Test(
        "{ if true {} else {} | let x = 0; }",
        SyntaxContextKind.Statement | SyntaxContextKind.PostExpression);

    [Fact]
    public void ParameterOrVariableAfterLet() => Test(
        "let |",
        SyntaxContextKind.ParameterOrVariable);

    [Fact]
    public void ParameterOrVariableOrExpressionAfterOpenParen() => Test(
        "(|",
        SyntaxContextKind.ParameterOrVariable | SyntaxContextKind.Expression);

    [Fact]
    public void ParameterOrVariableAfterOpenParenInFunction() => Test(
        "func f(|) {}",
        SyntaxContextKind.ParameterOrVariable);

    [Fact]
    public void ParameterOrVariableAfterCommaInParameterList() => Test(
        "(x, |) => x;",
        SyntaxContextKind.ParameterOrVariable);

    [Fact]
    public void InLoop() => Test(
        "loop {|}",
        SyntaxContextKind.InLoop | SyntaxContextKind.Expression | SyntaxContextKind.Statement);
}
