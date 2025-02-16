namespace Noa.Compiler.Parsing.Tests;

public class StatementTests
{
    [Fact]
    public Task Parses_AssignmentStatement_WithIdentifier() =>
        ParseTest.Test(
            "x = 0;",
            p => p.ParseRoot());

    [Fact]
    public Task Parses_AssignmentStatement_WithNumber_AndProduces_InvalidLValue() =>
        ParseTest.Test(
            "0 = 1;",
            p => p.ParseRoot());
}
