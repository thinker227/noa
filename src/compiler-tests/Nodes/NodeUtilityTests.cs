namespace Noa.Compiler.Nodes.Tests;

public class NodeUtilityTests
{
    [Fact]
    public void FindNodeAt_FindsNode_WithinChildren()
    {
        var text = """
        let x = 12 + 34 * 56;
        """;
        //           ^
        var position = 13;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        var node = ast.Root.FindNodeAt(position);

        var num = node.ShouldBeOfType<NumberExpression>();
        num.Value.ShouldBe(34);
    }

    [Fact]
    public void FindNodeAt_ReturnsNull_ForPositionOutsideNode()
    {
        var text = """
        let x = 0;
        """;
        var position = 20;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        var node = ast.Root.FindNodeAt(position);

        node.ShouldBeNull();
    }

    [Fact]
    public void FindNodeAt_ReturnsContainingNode_ForPositionInWhitespace()
    {
        var text = """
        let x = 0;
        """;
        //   ^
        var position = 5;
        var source = new Source(text, "test-input");
        var ast = Ast.Parse(source);

        var node = ast.Root.FindNodeAt(position);

        node.ShouldBeOfType<LetDeclaration>();
    }
}
