namespace Noa.Compiler.Parsing.Tests;

public sealed class IfExpressionTests
{
    [Fact]
    public Task Else_IsRequired_InExpressions() =>
        ParseTest.Test(
            "if true {}",
            p => p.ParseExpressionOrError());

    [Fact]
    public Task Else_IsNotRequired_InStatements() =>
        ParseTest.Test(
            "if true {}",
            p => p.ParseRoot());
    
    [Fact]
    public Task Else_IsAllowed_InExpressions() =>
        ParseTest.Test(
            "if true {} else {}",
            p => p.ParseExpressionOrError());

    [Fact]
    public Task IfWithoutElse_InBlock_ParsesAsStatement() =>
        ParseTest.Test(
            "if true {}",
            p => p.ParseRoot());
}
